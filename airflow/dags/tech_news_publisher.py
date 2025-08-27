"""
### Tech News Publisher DAG (SECURE VERSION)
This DAG performs a full ETL process with SECURE authentication:
1.  **Extract**: Scrapes the latest news from the TechCrunch RSS feed.
2.  **Transform**: Uses the Gemini API to generate a blog post summary from the news.
3.  **Load**: Publishes using a pre-generated, long-lived bot token (SECURE).

**Setup Required:**
- **Environment Variables**: 
  - `GEMINI_API_KEY` containing your Google AI Studio API key.
  - `BOT_TOKEN` containing the pre-generated JWT token for the bot user (SECURE)
  - `BLOG_API_URL` (optional, defaults to your BlogService URL)
"""
import pendulum
import logging
import feedparser
import google.generativeai as genai
import os
import requests

from airflow.decorators import dag, task


@dag(
    dag_id="tech_news_publisher",
    start_date=pendulum.datetime(2024, 1, 1, tz="UTC"),
    schedule="@daily",  # Run once a day
    catchup=False,
    tags=["tech-news", "scraper", "gemini", "blog", "ai", "secure"],
    doc_md=__doc__,
)
def tech_news_publisher_dag():

    @task
    def scrape_tech_news() -> list[dict]:
        """Fetches news from the TechCrunch RSS feed and returns a list of articles."""
        url = "https://techcrunch.com/feed/"
        logging.info(f"Fetching news from {url}...")

        feed = feedparser.parse(url)
        articles = [
            {"title": entry.title, "link": entry.link}
            for entry in feed.entries[:5]
        ]

        if not articles:
            raise ValueError("No articles found in the RSS feed.")

        logging.info(f"Successfully scraped {len(articles)} articles.")
        return articles

    @task
    def summarize_with_gemini(articles: list[dict]) -> dict:
        """Takes a list of articles, generates a blog post, and returns it as a dictionary."""
        api_key = os.getenv("GEMINI_API_KEY")
        if not api_key:
            raise ValueError("GEMINI_API_KEY environment variable is not set")

        genai.configure(api_key=api_key)
        model = genai.GenerativeModel('gemini-1.5-flash-latest')

        logging.info("Generating summary with Gemini...")

        article_list_str = "\n".join([f"- {a['title']}: {a['link']}" for a in articles])
        prompt = f"""
        You are a witty and engaging tech blogger.
        Based on the following list of today's top articles from TechCrunch, write a short and engaging blog post summarizing the key news.

        The blog post should have a clear, catchy title on the very first line.
        The tone should be conversational. Synthesize the articles into a cohesive narrative about what's happening in tech today.

        End with a friendly sign-off.

        Here are the articles:
        {article_list_str}
        """

        response = model.generate_content(prompt)
        blog_post = response.text

        try:
            title, body = blog_post.split('\n', 1)
        except ValueError:
            title = f"Today's Tech News Summary - {pendulum.now().to_date_string()}"
            body = blog_post

        post_data = {
            "title": title.strip().replace("*", ""),
            "content": body.strip(),
        }

        logging.info("Successfully generated blog post from Gemini.")
        logging.info(f"Generated title: {post_data['title']}")
        return post_data

    @task
    def publish_to_blog_api(blog_post_data: dict) -> dict:
        """Publishes the blog post using pre-generated secure bot token."""
        import requests
        import os

        # 🔒 SECURITY: Use pre-generated bot token (no dynamic token generation)
        bot_token = os.getenv("BOT_TOKEN")
        if not bot_token:
            raise ValueError("BOT_TOKEN environment variable is required")

        # 🔧 FIX: Handle both full URL and base URL from environment
        blog_api_url = os.getenv("BLOG_API_URL", "https://pancakes-blog.graygrass-374618f4.westeurope.azurecontainerapps.io")
        
        # If BLOG_API_URL already includes the endpoint, use it directly
        if "/api/blogposts" in blog_api_url or "/api/BlogPosts" in blog_api_url:
            full_url = blog_api_url
        else:
            # Otherwise append the endpoint
            full_url = f"{blog_api_url}/api/BlogPosts"
        
        headers = {
            "Authorization": f"Bearer {bot_token}",
            "Content-Type": "application/json"
        }

        payload = {
            "title": blog_post_data["title"],
            "content": blog_post_data["content"],
            # Default image for AI-generated posts (can be overridden if blog_post_data provides one)
            "imageUrl": blog_post_data.get("imageUrl", "default-post-ai.png"),
            "status": 1,  # Published
            "authorId": "BOT-AI-CONTENT-CREATOR-001", 
            "tags": ["AI-Generated", "Tech-News", "Daily"]
        }

        logging.info(f"📝 Publishing blog post to {full_url}")
        logging.info(f"📰 Title: {payload['title']}")

        response = requests.post(full_url, json=payload, headers=headers)
        
        if response.status_code in [200, 201]:
            result = response.json()
            logging.info(f"✅ Successfully published blog post ID: {result.get('id')}")
            return {
                "id": result.get("id"),
                "title": result.get("title"), 
                "status": "published",
                "authorName": result.get("authorName", "AI Content Creator")
            }
        else:
            logging.error(f"❌ Failed to publish: {response.status_code} - {response.text}")
            raise Exception(f"Failed to publish blog post: {response.status_code}")

    # 🔒 SECURE: Simple task flow with no dynamic authentication
    scraped_articles = scrape_tech_news()
    blog_post_data = summarize_with_gemini(scraped_articles)
    published_result = publish_to_blog_api(blog_post_data)

tech_news_publisher_dag()
