import React, { useState } from "react";
import { Form, Input, Button, Card, Typography, Space, message } from "antd";
import {
  ArrowLeftOutlined,
  SaveOutlined,
  SendOutlined,
} from "@ant-design/icons";
import { useRouter } from "@/router/RouterProvider";
import { useAuth } from "@/contexts/AuthContext";
import { blogPostsApi } from "@/services/blogApi";
import { PostStatus } from "@/types/blog";
import type { CreateBlogPostDto } from "@/types/blog";
import { TagInput, ImageUpload, MarkdownEditor } from "@/components/common";
import "./CreateBlogPage.scss";

const { Title } = Typography;

interface CreateBlogFormValues {
  title: string;
  content: string;
  featuredImage?: string;
  featuredImageUrl?: string;
  tags: string[];
}

const CreateBlogPage: React.FC = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [actionType, setActionType] = useState<"draft" | "publish" | null>(
    null
  );
  const { navigate } = useRouter();
  const { user, isAuthenticated } = useAuth();

  // Redirect to login if user is not authenticated
  React.useEffect(() => {
    if (!isAuthenticated) {
      message.warning("Please log in to create a blog post");
      navigate("login");
    }
  }, [isAuthenticated, navigate]);

  const handleSubmit = async (
    values: CreateBlogFormValues,
    isDraft: boolean = false
  ) => {
    // Check if user is authenticated
    if (!isAuthenticated || !user) {
      message.error("You must be logged in to create a blog post");
      navigate("login");
      return;
    }

    setLoading(true);
    setActionType(isDraft ? "draft" : "publish");

    try {
      const currentDate = new Date().toISOString();

      // Use uploaded image URL or external URL, with uploaded taking priority
      const featuredImage =
        values.featuredImage || values.featuredImageUrl || undefined;

      const createDto: CreateBlogPostDto = {
        title: values.title,
        content: values.content,
        featuredImage: featuredImage,
        status: isDraft ? PostStatus.Draft : PostStatus.Published,
        authorId: "00000000-0000-0000-0000-000000000000", // Placeholder GUID - backend will override with current user's ID
        publishedAt: isDraft ? undefined : currentDate,
        tags: values.tags || [],
      };

      console.log("Sending createDto:", createDto);

      await blogPostsApi.create(createDto);

      if (isDraft) {
        message.success("Blog post saved as draft!");
        navigate("drafts");
      } else {
        message.success("Blog post published successfully!");
        navigate("home");
      }
    } catch (error) {
      console.error("Error creating blog post:", error);
      message.error(
        `Failed to ${
          isDraft ? "save draft" : "publish blog post"
        }. Please try again.`
      );
    } finally {
      setLoading(false);
      setActionType(null);
    }
  };

  const handleSaveAsDraft = () => {
    form
      .validateFields()
      .then((values) => {
        handleSubmit(values, true);
      })
      .catch((errorInfo) => {
        // Only require title for drafts, content can be optional
        const titleErrors = errorInfo.errorFields?.filter(
          (field: unknown) => field.name[0] === "title"
        );
        if (titleErrors && titleErrors.length > 0) {
          message.error("Please enter a title to save as draft");
          return;
        }

        // Get current form values even if some validations fail
        const values = form.getFieldsValue();
        if (!values.title || values.title.trim() === "") {
          message.error("Please enter a title to save as draft");
          return;
        }

        handleSubmit(values, true);
      });
  };

  const handlePublish = () => {
    form.validateFields().then((values) => {
      handleSubmit(values, false);
    });
  };

  return (
    <div className="create-blog-page">
      <div className="create-blog-page__container">
        <div className="create-blog-page__header">
          <Button
            type="text"
            icon={<ArrowLeftOutlined />}
            onClick={() => navigate("home")}
            className="create-blog-page__back-button"
          >
            Back to Home
          </Button>
          <Title level={1} className="create-blog-page__title">
            Create New Blog Post
          </Title>
        </div>

        <Card className="create-blog-page__form-card">
          <Form
            form={form}
            layout="vertical"
            onFinish={handlePublish}
            className="create-blog-page__form"
          >
            <Form.Item
              label="Title"
              name="title"
              rules={[
                { required: true, message: "Please enter a title" },
                { max: 200, message: "Title must be less than 200 characters" },
              ]}
            >
              <Input
                placeholder="Enter blog post title"
                className="create-blog-page__input"
              />
            </Form.Item>

            <Form.Item
              label="Featured Image"
              name="featuredImage"
              help="Upload an image from your computer or enter a URL"
            >
              <ImageUpload
                placeholder="Upload or enter image URL"
                disabled={loading}
              />
            </Form.Item>

            <Form.Item
              label="Or enter image URL"
              name="featuredImageUrl"
              rules={[
                { max: 500, message: "URL must be less than 500 characters" },
                { type: "url", message: "Please enter a valid URL" },
              ]}
            >
              <Input
                placeholder="https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=800&h=400&fit=crop"
                className="create-blog-page__input"
              />
            </Form.Item>

            <Form.Item
              label="Content"
              name="content"
              rules={[
                { required: true, message: "Please enter content" },
                { min: 10, message: "Content must be at least 10 characters" },
              ]}
              help="Use Markdown formatting. The editor provides buttons to help you format your content."
            >
              <MarkdownEditor
                rows={12}
                placeholder="Write your blog post content here using Markdown..."
                className="create-blog-page__markdown-editor"
              />
            </Form.Item>

            <Form.Item
              label="Tags"
              name="tags"
              help="Add tags to help categorize your blog post"
            >
              <TagInput
                placeholder="Add tags..."
                maxTags={10}
                className="create-blog-page__tags"
              />
            </Form.Item>

            <Form.Item className="create-blog-page__submit-section">
              <Space size="middle">
                <Button
                  type="default"
                  onClick={() => navigate("home")}
                  disabled={loading}
                >
                  Cancel
                </Button>
                <Button
                  type="default"
                  icon={<SaveOutlined />}
                  loading={loading && actionType === "draft"}
                  disabled={loading}
                  onClick={handleSaveAsDraft}
                  className="create-blog-page__draft-button"
                >
                  Save as Draft
                </Button>
                <Button
                  type="primary"
                  htmlType="submit"
                  icon={<SendOutlined />}
                  loading={loading && actionType === "publish"}
                  disabled={loading}
                  className="create-blog-page__submit-button"
                >
                  Publish
                </Button>
              </Space>
            </Form.Item>
          </Form>
        </Card>
      </div>
    </div>
  );
};

export default CreateBlogPage;
