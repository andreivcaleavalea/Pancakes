import React, { useState } from "react";
import { Button, Input, Form, App } from "antd";
import { SendOutlined } from "@ant-design/icons";
import type { CommentFormProps } from "@/types/comment";
import "./CommentForm.scss";

const { TextArea } = Input;

const CommentForm: React.FC<CommentFormProps> = ({
  blogPostId,
  parentCommentId,
  parentAuthor,
  onSubmit,
  onCancel,
  loading = false,
  placeholder,
}) => {
  const [form] = Form.useForm();
  const [submitting, setSubmitting] = useState(false);
  const { message } = App.useApp();

  const handleSubmit = async (values: { content: string }) => {
    try {
      setSubmitting(true);
      await onSubmit({
        content: values.content,
        blogPostId,
        parentCommentId,
        // No need to send authorName or authorId - backend will populate from JWT token
      });
      form.resetFields();
      message.success(
        parentCommentId
          ? "Reply added successfully!"
          : "Comment added successfully!"
      );
      if (onCancel) onCancel(); // Close reply form after successful submission
    } catch (error) {
      message.error(
        `Failed to add ${
          parentCommentId ? "reply" : "comment"
        }. Please try again.`
      );
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div
      className={`comment-form ${parentCommentId ? "comment-form--reply" : ""}`}
    >
      {parentAuthor && (
        <div className="comment-form__reply-header">
          Replying to <strong>@{parentAuthor}</strong>
        </div>
      )}
      <Form form={form} onFinish={handleSubmit} layout="vertical">
        <Form.Item
          name="content"
          rules={[
            { required: true, message: "Please enter your comment" },
            { min: 1, message: "Comment cannot be empty" },
            { max: 1000, message: "Comment cannot exceed 1000 characters" },
          ]}
        >
          <TextArea
            placeholder={
              placeholder ||
              (parentCommentId
                ? "Write a reply..."
                : "Share your thoughts about this recipe...")
            }
            rows={parentCommentId ? 3 : 4}
            maxLength={1000}
            showCount
            disabled={loading || submitting}
          />
        </Form.Item>

        <Form.Item className="comment-form__submit">
          <div className="comment-form__buttons">
            <Button
              type="primary"
              htmlType="submit"
              icon={<SendOutlined />}
              loading={loading || submitting}
              size={parentCommentId ? "middle" : "large"}
            >
              {submitting
                ? "Posting..."
                : parentCommentId
                ? "Reply"
                : "Post Comment"}
            </Button>
            {onCancel && (
              <Button
                type="default"
                onClick={onCancel}
                disabled={loading || submitting}
                size={parentCommentId ? "middle" : "large"}
              >
                Cancel
              </Button>
            )}
          </div>
        </Form.Item>
      </Form>
    </div>
  );
};

export default CommentForm;
