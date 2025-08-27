import React, { useState, useRef } from "react";
import { Input, Button, Space, Tooltip, Divider } from "antd";
import {
  BoldOutlined,
  ItalicOutlined,
  UnderlineOutlined,
  LinkOutlined,
  PictureOutlined,
  UnorderedListOutlined,
  OrderedListOutlined,
  FontSizeOutlined,
  CodeOutlined,
  TableOutlined,
  EyeOutlined,
  EditOutlined,
} from "@ant-design/icons";
import { MarkdownRenderer } from "@/components/common/MarkdownRenderer/MarkdownRenderer";
import "./MarkdownEditor.scss";

const { TextArea } = Input;

interface MarkdownEditorProps {
  value?: string;
  onChange?: (value: string) => void;
  placeholder?: string;
  rows?: number;
  disabled?: boolean;
  className?: string;
}

export const MarkdownEditor: React.FC<MarkdownEditorProps> = ({
  value = "",
  onChange,
  placeholder = "Write your content here...",
  rows = 12,
  disabled = false,
  className = "",
}) => {
  const [isPreview, setIsPreview] = useState(false);
  const textAreaRef = useRef<{ resizableTextArea?: { textArea?: HTMLTextAreaElement } }>(null);

  const insertText = (before: string, after: string = "", placeholder: string = "") => {
    const textArea = textAreaRef.current?.resizableTextArea?.textArea;
    if (!textArea) return;

    const start = textArea.selectionStart;
    const end = textArea.selectionEnd;
    const selectedText = value.substring(start, end);
    const textToInsert = selectedText || placeholder;
    
    const newText = 
      value.substring(0, start) + 
      before + 
      textToInsert + 
      after + 
      value.substring(end);
    
    onChange?.(newText);

    // Set cursor position after insertion
    setTimeout(() => {
      const newCursorPos = start + before.length + textToInsert.length;
      textArea.setSelectionRange(newCursorPos, newCursorPos);
      textArea.focus();
    }, 0);
  };

  const insertAtCursor = (text: string) => {
    const textArea = textAreaRef.current?.resizableTextArea?.textArea;
    if (!textArea) return;

    const start = textArea.selectionStart;
    const newText = value.substring(0, start) + text + value.substring(start);
    
    onChange?.(newText);

    setTimeout(() => {
      textArea.setSelectionRange(start + text.length, start + text.length);
      textArea.focus();
    }, 0);
  };

  const formatButtons = [
    {
      icon: <BoldOutlined />,
      tooltip: "Bold",
      action: () => insertText("**", "**", "bold text"),
    },
    {
      icon: <ItalicOutlined />,
      tooltip: "Italic", 
      action: () => insertText("*", "*", "italic text"),
    },
    {
      icon: <UnderlineOutlined />,
      tooltip: "Strikethrough",
      action: () => insertText("~~", "~~", "strikethrough text"),
    },
    {
      icon: <CodeOutlined />,
      tooltip: "Inline Code",
      action: () => insertText("`", "`", "code"),
    },
  ];

  const structureButtons = [
    {
      icon: <FontSizeOutlined />,
      tooltip: "Heading",
      action: () => insertAtCursor("## "),
    },
    {
      icon: <UnorderedListOutlined />,
      tooltip: "Bullet List",
      action: () => insertAtCursor("- "),
    },
    {
      icon: <OrderedListOutlined />,
      tooltip: "Numbered List", 
      action: () => insertAtCursor("1. "),
    },
    {
      icon: <LinkOutlined />,
      tooltip: "Link",
      action: () => insertText("[", "](url)", "link text"),
    },
    {
      icon: <PictureOutlined />,
      tooltip: "Image",
      action: () => insertText("![", "](image-url)", "alt text"),
    },
    {
      icon: <TableOutlined />,
      tooltip: "Table",
      action: () => insertAtCursor(
        "| Column 1 | Column 2 | Column 3 |\n|----------|----------|----------|\n| Cell 1   | Cell 2   | Cell 3   |\n"
      ),
    },
  ];

  const moreButtons = [
    {
      text: "Code Block",
      action: () => insertText("```\n", "\n```", "code here"),
    },
    {
      text: "Quote",
      action: () => insertAtCursor("> "),
    },
    {
      text: "Horizontal Rule",
      action: () => insertAtCursor("\n---\n"),
    },
  ];

  return (
    <div className={`markdown-editor ${className}`}>
      <div className="markdown-editor__toolbar">
        <Space wrap>
          {/* Format buttons */}
          <Space>
            {formatButtons.map((button, index) => (
              <Tooltip key={index} title={button.tooltip}>
                <Button
                  type="text"
                  icon={button.icon}
                  onClick={button.action}
                  disabled={disabled}
                  size="small"
                />
              </Tooltip>
            ))}
          </Space>

          <Divider type="vertical" />

          {/* Structure buttons */}
          <Space>
            {structureButtons.map((button, index) => (
              <Tooltip key={index} title={button.tooltip}>
                <Button
                  type="text"
                  icon={button.icon}
                  onClick={button.action}
                  disabled={disabled}
                  size="small"
                />
              </Tooltip>
            ))}
          </Space>

          <Divider type="vertical" />

          {/* More buttons */}
          <Space>
            {moreButtons.map((button, index) => (
              <Tooltip key={index} title={button.text}>
                <Button
                  type="text"
                  onClick={button.action}
                  disabled={disabled}
                  size="small"
                >
                  {button.text}
                </Button>
              </Tooltip>
            ))}
          </Space>

          <Divider type="vertical" />

          {/* Preview toggle */}
          <Tooltip title={isPreview ? "Edit Mode" : "Preview Mode"}>
            <Button
              type={isPreview ? "primary" : "text"}
              icon={isPreview ? <EditOutlined /> : <EyeOutlined />}
              onClick={() => setIsPreview(!isPreview)}
              disabled={disabled}
              size="small"
            >
              {isPreview ? "Edit" : "Preview"}
            </Button>
          </Tooltip>
        </Space>
      </div>

      <div className="markdown-editor__content">
        {isPreview ? (
          <div className="markdown-editor__preview">
            <MarkdownRenderer content={value} />
          </div>
        ) : (
          <TextArea
            ref={textAreaRef}
            value={value}
            onChange={(e) => onChange?.(e.target.value)}
            placeholder={placeholder}
            rows={rows}
            disabled={disabled}
            className="markdown-editor__textarea"
          />
        )}
      </div>

      {!isPreview && (
        <div className="markdown-editor__help">
          <small>
            <strong>Tip:</strong> You can use Markdown syntax. Use the buttons above for quick formatting.
          </small>
        </div>
      )}
    </div>
  );
};

export default MarkdownEditor;
