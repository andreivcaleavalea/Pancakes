import React, { useState, useEffect } from "react";
import {
  Form,
  Input,
  Button,
  Card,
  Typography,
  Space,
  message,
  Spin,
} from "antd";
import { ArrowLeftOutlined, SaveOutlined } from "@ant-design/icons";
import { useRouter } from "@/router/RouterProvider";
import { useAuth } from "@/contexts/AuthContext";
import { blogPostsApi } from "@/services/blogApi";
import { PostStatus } from "@/types/blog";
import type { UpdateBlogPostDto, BlogPost } from "@/types/blog";
import { TagInput } from "@/components/common";
import "./EditBlogPage.scss";

const { Title } = Typography;
const { TextArea } = Input;

interface EditBlogFormValues {
  title: string;
  content: string;
  featuredImage?: string;
  tags: string[];
}

const EditBlogPage: React.FC = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);
  const [blogPost, setBlogPost] = useState<BlogPost | null>(null);
  const { navigate, blogId } = useRouter();
  const { user, isAuthenticated } = useAuth();

  // Load existing blog post data
  useEffect(() => {
    const loadBlogPost = async () => {
      if (!blogId) {
        message.error("Blog post ID is required");
        navigate("home");
        return;
      }

      try {
        setInitialLoading(true);
        const post = await blogPostsApi.getById(blogId);

        // Check if the current user is the author
        if (!user || post.authorId !== user.id) {
          message.error("You can only edit your own blog posts");
          navigate("home");
          return;
        }

        setBlogPost(post);

        // Set form values
        form.setFieldsValue({
          title: post.title,
          content: post.content,
          featuredImage: post.featuredImage || "",
          tags: post.tags || [],
        });
      } catch (error) {
        console.error("Error loading blog post:", error);
        message.error("Failed to load blog post");
        navigate("home");
      } finally {
        setInitialLoading(false);
      }
    };

    if (isAuthenticated && user) {
      loadBlogPost();
    }
  }, [blogId, isAuthenticated, user, navigate, form]);

  // Redirect to login if user is not authenticated
  useEffect(() => {
    if (!isAuthenticated) {
      message.warning("Please log in to edit blog posts");
      navigate("login");
    }
  }, [isAuthenticated, navigate]);

  const handleSubmit = async (values: EditBlogFormValues) => {
    if (!isAuthenticated || !user || !blogPost || !blogId) {
      message.error("You must be logged in to edit blog posts");
      navigate("login");
      return;
    }

    setLoading(true);
    try {
      const updateDto: UpdateBlogPostDto = {
        title: values.title,
        content: values.content,
        featuredImage: values.featuredImage || undefined,
        status: PostStatus.Published, // Keep as published
        tags: values.tags || [],
      };

      console.log("Sending updateDto:", updateDto);

      await blogPostsApi.update(blogId, updateDto);
      message.success("Blog post updated successfully!");
      navigate("blog-view", undefined, blogId);
    } catch (error) {
      console.error("Error updating blog post:", error);
      message.error("Failed to update blog post. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  if (initialLoading) {
    return (
      <div className="edit-blog-page">
        <div className="edit-blog-page__container">
          <Card className="edit-blog-page__loading-card">
            <Spin size="large" />
            <Typography.Text
              style={{ marginTop: 16, display: "block", textAlign: "center" }}
            >
              Loading blog post...
            </Typography.Text>
          </Card>
        </div>
      </div>
    );
  }

  if (!blogPost) {
    return null; // This shouldn't happen due to the redirect logic above
  }

  return (
    <div className="edit-blog-page">
      <div className="edit-blog-page__container">
        <div className="edit-blog-page__header">
          <Button
            type="text"
            icon={<ArrowLeftOutlined />}
            onClick={() => navigate("blog-view", undefined, blogId)}
            className="edit-blog-page__back-button"
          >
            Back to Post
          </Button>
          <Title level={1} className="edit-blog-page__title">
            Edit Blog Post
          </Title>
        </div>

        <Card className="edit-blog-page__form-card">
          <Form
            form={form}
            layout="vertical"
            onFinish={handleSubmit}
            className="edit-blog-page__form"
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
                className="edit-blog-page__input"
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
                className="edit-blog-page__input"
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
                className="edit-blog-page__textarea"
              />
            </Form.Item>

            <Form.Item
              label="Tags"
              name="tags"
              help="Edit tags to help categorize your blog post"
            >
              <TagInput
                placeholder="Add tags..."
                maxTags={10}
                className="edit-blog-page__tags"
              />
            </Form.Item>

            <Form.Item className="edit-blog-page__submit-section">
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
                  className="edit-blog-page__submit-button"
                >
                  Update Blog Post
                </Button>
              </Space>
            </Form.Item>
          </Form>
        </Card>
      </div>
    </div>
  );
};

export default EditBlogPage;
