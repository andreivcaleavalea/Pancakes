import React from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import "./MarkdownRenderer.scss";

interface MarkdownRendererProps {
  content: string;
  className?: string;
}

// Centralized markdown renderer so we can control allowed elements / future sanitization.
export const MarkdownRenderer: React.FC<MarkdownRendererProps> = ({ content, className }) => {
  // Basic defensive trimming; backend already stores markdown text.
  const safe = content || "";
  return (
    <div className={`markdown-body ${className || ""}`.trim()}>
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        // Optionally restrict allowed elements via allowedElements / disallowedElements.
      >
        {safe}
      </ReactMarkdown>
    </div>
  );
};

export default MarkdownRenderer;
