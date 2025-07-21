import React, { useState } from "react";
import { Form, Input, Button, Card, Typography, Space, message } from "antd";
import { ArrowLeftOutlined, SaveOutlined } from "@ant-design/icons";
import { useRouter } from "@/router/RouterProvider";
import { blogPostsApi } from "@/services/blogApi";
import { PostStatus } from "@/types/blog";
import type { CreateBlogPostDto } from "@/types/blog";
import "./CreateBlogPage.scss";

const { Title } = Typography;
const { TextArea } = Input;

interface CreateBlogFormValues {
  title: string;
  content: string;
  featuredImage?: string;
}

const CreateBlogPage: React.FC = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const { navigate } = useRouter();

  const handleSubmit = async (values: CreateBlogFormValues) => {
    setLoading(true);
    try {
      const currentDate = new Date().toISOString();

      // Generate a proper UUID for author ID (temporary solution)
      const tempAuthorId = "123e4567-e89b-12d3-a456-426614174000";

      const createDto: CreateBlogPostDto = {
        title: values.title,
        content: values.content,
        featuredImage: values.featuredImage || undefined,
        status: PostStatus.Published, // Always set to published (status code 1)
        authorId: tempAuthorId, // TODO: Get from JWT token
        publishedAt: currentDate,
      };

      console.log("Sending createDto:", createDto);

      await blogPostsApi.create(createDto);
      message.success("Blog post created successfully!");
      navigate("home");
    } catch (error) {
      console.error("Error creating blog post:", error);
      message.error("Failed to create blog post. Please try again.");
    } finally {
      setLoading(false);
    }
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
            onFinish={handleSubmit}
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
              label="Featured Image URL"
              name="featuredImage"
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
            >
              <TextArea
                rows={12}
                placeholder="Write your blog post content here..."
                className="create-blog-page__textarea"
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
                  type="primary"
                  htmlType="submit"
                  icon={<SaveOutlined />}
                  loading={loading}
                  className="create-blog-page__submit-button"
                >
                  Create Blog Post
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
