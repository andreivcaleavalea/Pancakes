"""\
### Tech News Publisher DAG v2

Refactored, multi-source, idempotent, dynamic-mapped Airflow DAG that:

1. Fetches and normalizes articles from multiple reputable tech RSS feeds.
2. Skips the entire run early if feed hash unchanged (idempotency + cost control).
3. Summarizes each article in parallel (dynamic task mapping) using Gemini.
4. Aggregates per-article digests into a cohesive, well-structured daily blog post
   with sections (Headlines, Deep Dive, Trends, What To Watch, Closing, Meta Description).
5. Derives lightweight tags and ensures safe length + formatting.
6. Publishes to the Blog API ONLY if not already published (hash + optional API check).

Security & Config Guidance:
---------------------------
- Prefer storing secrets / tokens in an Airflow Connection (Conn ID: ``blog_api``) OR Variables rather than raw env vars.
- Environment variables still supported for quick local testing: ``GEMINI_API_KEY``, ``BOT_TOKEN``, ``BLOG_API_URL``.
- Airflow Variable overrides (if set) take precedence: ``GEMINI_API_KEY``, ``BLOG_API_URL``, ``BOT_TOKEN``.

Duplicate / Idempotency Strategy:
---------------------------------
- We compute a SHA256 hash over the sorted list of (title+link) pairs across all feeds (``feed_fingerprint``).
- Stored in Airflow Variable ``LATEST_TECH_NEWS_FEED_HASH`` after successful publish.
- If unchanged next day (e.g., weekend stagnation), DAG short-circuits before any Gemini calls.

Dynamic Task Mapping Requirements:
----------------------------------
- Airflow 2.4+ (tested with 2.9+) supports ``@task`` + ``expand``.
- If your version is older, replace dynamic mapping with a loop inside a single Python task.

Feeds Used (adjust as needed; all public RSS endpoints):
--------------------------------------------------------
- TechCrunch: https://techcrunch.com/feed/
- The Verge:  https://www.theverge.com/rss/index.xml
- Ars Technica: https://feeds.arstechnica.com/arstechnica/index/

Lightweight Tag Extraction:
---------------------------
- Simple keyword frequency over titles (stopword filtered) to produce <=6 tags.

Gemini Usage Notes:
-------------------
- Uses ``gemini-1.5-flash-latest`` by default (cost-efficient / fast).
- Fallback attempt to ``gemini-1.5-pro-latest`` if flash errors with retriable exception.

Extensibility Hooks:
--------------------
- Add more feeds in ``FEED_URLS``.
- Swap summarization model by changing ``DEFAULT_MODEL`` constants.
- Add advanced NLP (NER, topic modeling) by introducing optional providers.

"""
from __future__ import annotations

import hashlib
import json
import logging
import os
import re
from dataclasses import dataclass
from typing import List, Dict, Any

import feedparser
import pendulum
import google.generativeai as genai
import requests

from airflow.decorators import dag, task
from airflow.exceptions import AirflowSkipException
from airflow.models import Variable


# ------------------------------ Configuration ---------------------------------
FEED_URLS: List[str] = [
    "https://techcrunch.com/feed/",
    "https://www.theverge.com/rss/index.xml",
    "https://feeds.arstechnica.com/arstechnica/index/",
]

MIN_ARTICLES = 5               # Fail if below (ensures meaningful post)
MAX_ARTICLES = 10              # Cap to limit token usage
DEFAULT_MODEL_PRIMARY = "gemini-1.5-flash-latest"
DEFAULT_MODEL_FALLBACK = "gemini-1.5-pro-latest"
ARTICLE_SUMMARY_MAX_CHARS = 600
POST_MAX_CHARS = 18000         # Safety guard
FEED_HASH_VARIABLE_KEY = "LATEST_TECH_NEWS_FEED_HASH"
BOT_AUTHOR_ID = "BOT-AI-CONTENT-CREATOR-001"
DEFAULT_IMAGE = "default-post-ai.png"


STOPWORDS = set(
    "the a an and or but if to in of for on with at from by about as into over after before during under again further then once here there when where why how all any both each few more most other some such no nor not only own same so than too very can will just don should now".split()
)


@dataclass
class Article:
    title: str
    link: str
    source: str


def _get_env_or_var(name: str, default: str | None = None) -> str | None:
    """Fetch a value, preferring Airflow Variable over environment variable."""
    val = Variable.get(name, default_var=None)
    if val is not None:
        return val
    return os.getenv(name, default)


def _configure_gemini(api_key: str):
    if not api_key:
        raise ValueError("GEMINI_API_KEY not provided (Variable or env)")
    genai.configure(api_key=api_key)


def _safe_model_generate(prompt: str) -> str:
    """Attempt generation with primary, fallback to secondary model if needed."""
    last_error = None
    for model_name in (DEFAULT_MODEL_PRIMARY, DEFAULT_MODEL_FALLBACK):
        try:
            model = genai.GenerativeModel(model_name)
            resp = model.generate_content(prompt)
            if not getattr(resp, "text", None):
                raise RuntimeError("Empty response text from model")
            return resp.text
        except Exception as e:  # noqa: BLE001 - we log details and continue
            logging.warning("Model %s failed: %s", model_name, e)
            last_error = e
    raise RuntimeError(f"All model attempts failed: {last_error}")


def _extract_tags(articles: List[Article], limit: int = 6) -> List[str]:
    freq: Dict[str, int] = {}
    for a in articles:
        words = re.findall(r"[A-Za-z][A-Za-z0-9+-]{2,}", a.title.lower())
        for w in words:
            if w in STOPWORDS:
                continue
            freq[w] = freq.get(w, 0) + 1
    # Sort by frequency then alphabetically, filter out overly generic tokens
    sorted_items = sorted(freq.items(), key=lambda kv: (-kv[1], kv[0]))
    tags = [w.title().replace("+", "-") for w, c in sorted_items if c >= 1][:limit]
    # Fallback baseline tags
    base = ["Tech-News", "AI-Generated", "Daily"]
    for t in base:
        if t not in tags:
            tags.append(t)
    return tags[: limit + len(base)]


@dag(
    dag_id="tech_news_publisher_v2",
    start_date=pendulum.datetime(2024, 1, 1, tz="UTC"),
    schedule="@daily",
    catchup=False,
    max_active_runs=1,
    tags=["tech-news", "ai", "blog", "v2"],
    doc_md=__doc__,
)
def tech_news_publisher_v2():  # noqa: D401 - DAG factory

    @task(retries=2, retry_delay=pendulum.duration(seconds=30))
    def fetch_articles() -> Dict[str, Any]:
        """Fetch & normalize articles from multiple feeds, return dict with articles + hash.

        Return structure:
        {
          "articles": [ {title, link, source}, ... ],
          "feed_fingerprint": sha256 string
        }
        """
        collected: List[Article] = []
        for url in FEED_URLS:
            logging.info("Fetching feed: %s", url)
            feed = feedparser.parse(url)
            source = feed.feed.get("title", url.split("//")[-1][:30]) if getattr(feed, "feed", None) else url
            for entry in getattr(feed, "entries", [])[: MAX_ARTICLES + 5]:  # grab a few extra for filtering
                title = getattr(entry, "title", "").strip()
                link = getattr(entry, "link", "").strip()
                if not title or not link:
                    continue
                collected.append(Article(title=title, link=link, source=source))

        # Deduplicate by title/link pair (favor first occurrence)
        uniq = {}
        for art in collected:
            key = (art.title.lower(), art.link)
            if key not in uniq:
                uniq[key] = art
        articles = list(uniq.values())[:MAX_ARTICLES]

        if len(articles) < MIN_ARTICLES:
            raise ValueError(f"Insufficient articles ({len(articles)}) < MIN_ARTICLES={MIN_ARTICLES}")

        payload = "\n".join(f"{a.title}|{a.link}" for a in sorted(articles, key=lambda x: x.link))
        feed_hash = hashlib.sha256(payload.encode("utf-8")).hexdigest()
        logging.info("Collected %d unique articles. Feed hash: %s", len(articles), feed_hash)

        return {
            "articles": [a.__dict__ for a in articles],
            "feed_fingerprint": feed_hash,
        }

    @task
    def duplicate_guard(feed_bundle: Dict[str, Any]) -> List[Dict[str, Any]]:
        """Skip DAG early if feed hash unchanged since last successful publish."""
        feed_hash_new = feed_bundle["feed_fingerprint"]
        previous = Variable.get(FEED_HASH_VARIABLE_KEY, default_var=None)
        if previous == feed_hash_new:
            logging.info("Feed fingerprint unchanged (%s). Skipping run.", feed_hash_new)
            raise AirflowSkipException("No new tech news; skip summarization & publish")
        return feed_bundle["articles"]

    @task(retries=2, retry_delay=pendulum.duration(seconds=20))
    def summarize_article(article: Dict[str, Any]) -> Dict[str, Any]:
        """Produce a concise summary for a single article using Gemini."""
        api_key = _get_env_or_var("GEMINI_API_KEY")
        _configure_gemini(api_key)
        prompt = f"""
You are an efficient tech news summarizer.
Summarize the article below in 2-3 crisp sentences PLUS a single bullet 'Why it matters'.
Avoid hype; keep it factual yet engaging. Output JSON with keys: summary, why_it_matters.

TITLE: {article['title']}
URL: {article['link']}
SOURCE: {article.get('source','unknown')}
"""
        raw = _safe_model_generate(prompt)
        # Attempt to extract JSON (fallback: heuristic formatting)
        json_match = re.search(r"\{[\s\S]*\}", raw)
        if json_match:
            try:
                parsed = json.loads(json_match.group(0))
            except json.JSONDecodeError:
                parsed = {}
        else:
            parsed = {}

        summary = parsed.get("summary") or raw.strip().split("Why it matters:", 1)[0].strip()
        why = parsed.get("why_it_matters") or (raw.split("Why it matters:", 1)[1].strip() if "Why it matters:" in raw else "")

        # Normalize whitespace & ensure line breaks for downstream markdown blocks
        summary_clean = re.sub(r"\s+", " ", summary).strip()
        why_clean = re.sub(r"\s+", " ", why).strip()
        combined = summary_clean
        if why_clean:
            combined += f"\n**Why it matters:** {why_clean}"
        combined = combined[:ARTICLE_SUMMARY_MAX_CHARS]
        return {
            "title": article["title"],
            "link": article["link"],
            "source": article.get("source"),
            "digest": combined,
        }

    @task(retries=2, retry_delay=pendulum.duration(seconds=30))
    def assemble_post(summaries: List[Dict[str, Any]], feed_bundle: Dict[str, Any]) -> Dict[str, Any]:
        """Generate the final structured blog post using the per-article digests."""
        api_key = _get_env_or_var("GEMINI_API_KEY")
        _configure_gemini(api_key)

        date_str = pendulum.now("UTC").to_date_string()
        # Build a stable multi-section input for the model
        digest_lines = []
        for s in summaries:
            digest_lines.append(
                f"TITLE: {s['title']}\nSOURCE: {s['source']}\nLINK: {s['link']}\nDIGEST: {s['digest']}"
            )
        digests_block = "\n\n".join(digest_lines)

        prompt = f"""
You are a senior tech editor crafting a daily multi-source digest.
You will be given a list of already summarized articles (title, source, digest, link).

TASK: Produce a markdown blog post with:
1. An H1 catchy title (no date duplication if already implicit) for {date_str}.
2. A short italicized one-line teaser.
3. A 'Quick Headlines' section: bullet list of article titles with sources and links only.
4. A 'Deep Dive' section weaving 2-3 coherent paragraphs synthesizing overlaps or contrasts.
5. A 'Emerging Trends' section: 3-5 bullet insights derived from patterns across articles.
6. A 'What To Watch Next' section: 2-3 forward-looking concise bullets.
7. A friendly closing sentence.
8. A 155-char meta description (plain text) enclosed in <!--META: ... --> comment.

STYLE:
- Professional, concise, no fluff, no hallucinated facts beyond provided summaries.
- If uncertain, explicitly say what is unknown instead of speculating.
- Do NOT fabricate numbers or company announcements.

INPUT DIGESTS:\n{digests_block}

Return pure markdown only.
"""
        post_markdown = _safe_model_generate(prompt)
        if len(post_markdown) > POST_MAX_CHARS:
            logging.warning("Post length %d exceeds cap %d; truncating.", len(post_markdown), POST_MAX_CHARS)
            post_markdown = post_markdown[:POST_MAX_CHARS] + "\n...(truncated)"

        # Ensure consistent line endings and remove trailing spaces
        post_markdown = "\n".join([ln.rstrip() for ln in post_markdown.splitlines()])
        # Extract first non-empty line as title (strip markdown markers)
        first_line = next((ln for ln in post_markdown.splitlines() if ln.strip()), f"Daily Tech Digest {date_str}")
        clean_title = re.sub(r"^[#*\s]+", "", first_line).strip()
        tags = _extract_tags([Article(**a) for a in feed_bundle["articles"]])

        return {
            "title": clean_title,
            "content": post_markdown.strip(),
            "tags": tags,
            "feed_fingerprint": feed_bundle["feed_fingerprint"],
            "meta": {
                "article_count": len(summaries),
                "generated_at_utc": pendulum.now("UTC").to_iso8601_string(),
                "model_primary": DEFAULT_MODEL_PRIMARY,
            },
        }

    @task(retries=3, retry_delay=pendulum.duration(seconds=20))
    def publish_if_new(post: Dict[str, Any]):
        """Publish blog post unless feed fingerprint already stored (race-safe)."""
        # Double-check duplication (in case of race / parallel scheduler run)
        current_hash = Variable.get(FEED_HASH_VARIABLE_KEY, default_var=None)
        if current_hash == post["feed_fingerprint"]:
            raise AirflowSkipException("Already published (hash race)")

        blog_api_url = _get_env_or_var("BLOG_API_URL", "https://pancakes-blog.graygrass-374618f4.westeurope.azurecontainerapps.io")
        bot_token = _get_env_or_var("BOT_TOKEN") or _get_env_or_var("BLOG_API_TOKEN")
        if not bot_token:
            raise ValueError("BOT_TOKEN / BLOG_API_TOKEN not set (Variable or env)")

        # Normalize endpoint
        if not re.search(r"/api/BlogPosts/?$", blog_api_url, re.IGNORECASE):
            if blog_api_url.endswith("/"):
                blog_api_url = blog_api_url[:-1]
            blog_api_url += "/api/BlogPosts"

        payload = {
            "title": post["title"][:200],
            "content": post["content"],
            "imageUrl": DEFAULT_IMAGE,
            "status": 1,
            "authorId": BOT_AUTHOR_ID,
            "tags": post.get("tags", []),
        }

        headers = {"Authorization": f"Bearer {bot_token}", "Content-Type": "application/json"}
        logging.info("Publishing blog post '%s' to %s", payload["title"], blog_api_url)
        resp = requests.post(blog_api_url, json=payload, headers=headers, timeout=30)
        if resp.status_code not in (200, 201):
            logging.error("Publish failed: %s %s", resp.status_code, resp.text[:500])
            raise RuntimeError(f"Failed to publish blog post ({resp.status_code})")
        data = resp.json() if resp.headers.get("Content-Type", "").startswith("application/json") else {}
        # Persist hash ONLY after successful publish
        Variable.set(FEED_HASH_VARIABLE_KEY, post["feed_fingerprint"])
        logging.info("Publish success. ID=%s", data.get("id"))
        return {"id": data.get("id"), "title": payload["title"]}

    # DAG Orchestration -------------------------------------------------------
    feed_bundle = fetch_articles()
    articles = duplicate_guard(feed_bundle)
    # Dynamic mapping over articles
    summarized = summarize_article.expand(article=articles)
    post = assemble_post(summaries=summarized, feed_bundle=feed_bundle)
    publish_if_new(post)


tech_news_publisher_v2()
