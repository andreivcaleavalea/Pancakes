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
  Tag,
  Alert,
} from "antd";
import {
  ArrowLeftOutlined,
  SaveOutlined,
  SendOutlined,
  FileTextOutlined,
} from "@ant-design/icons";
import { useRouter } from "@/router/RouterProvider";
import { useAuth } from "@/contexts/AuthContext";
import { blogPostsApi } from "@/services/blogApi";
import { PostStatus } from "@/types/blog";
import type { UpdateBlogPostDto, BlogPost } from "@/types/blog";
import { TagInput, ImageUpload, MarkdownEditor } from "@/components/common";
import "./EditBlogPage.scss";

const { Title } = Typography;

interface EditBlogFormValues {
  title: string;
  content: string;
  featuredImage?: string;
  featuredImageUrl?: string;
  tags: string[];
}

const EditBlogPage: React.FC = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);
  const [blogPost, setBlogPost] = useState<BlogPost | null>(null);
  const [actionType, setActionType] = useState<
    "save" | "publish" | "draft" | null
  >(null);
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
          featuredImage:
            post.featuredImage &&
            post.featuredImage.includes("/assets/blog-images/")
              ? post.featuredImage
              : "",
          featuredImageUrl:
            post.featuredImage &&
            !post.featuredImage.includes("/assets/blog-images/")
              ? post.featuredImage
              : "",
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

  const handleSubmit = async (
    values: EditBlogFormValues,
    targetStatus?: PostStatus
  ) => {
    if (!isAuthenticated || !user || !blogPost || !blogId) {
      message.error("You must be logged in to edit blog posts");
      navigate("login");
      return;
    }

    setLoading(true);
    const status = targetStatus !== undefined ? targetStatus : blogPost.status;
    setActionType(
      status === PostStatus.Published
        ? "publish"
        : status === PostStatus.Draft
        ? "draft"
        : "save"
    );

    try {
      // Use uploaded image URL or external URL, with uploaded taking priority
      const featuredImage =
        values.featuredImage || values.featuredImageUrl || undefined;

      const updateDto: UpdateBlogPostDto = {
        title: values.title,
        content: values.content,
        featuredImage: featuredImage,
        status: status,
        tags: values.tags || [],
      };

      console.log("Sending updateDto:", updateDto);

      await blogPostsApi.update(blogId, updateDto);

      if (
        status === PostStatus.Published &&
        blogPost.status === PostStatus.Draft
      ) {
        message.success("Draft published successfully!");
      } else if (
        status === PostStatus.Draft &&
        blogPost.status === PostStatus.Published
      ) {
        message.success("Post converted to draft!");
      } else {
        message.success("Blog post updated successfully!");
      }

      // Update local state
      setBlogPost({ ...blogPost, ...updateDto });

      // Navigate based on the action
      if (status === PostStatus.Published) {
        navigate("blog-view", undefined, blogId);
      } else {
        navigate("drafts");
      }
    } catch (error) {
      console.error("Error updating blog post:", error);
      message.error("Failed to update blog post. Please try again.");
    } finally {
      setLoading(false);
      setActionType(null);
    }
  };

  const handleSaveChanges = () => {
    form.validateFields().then((values) => {
      handleSubmit(values);
    });
  };

  const handlePublish = () => {
    form.validateFields().then((values) => {
      handleSubmit(values, PostStatus.Published);
    });
  };

  const handleConvertToDraft = () => {
    form
      .validateFields()
      .then((values) => {
        handleSubmit(values, PostStatus.Draft);
      })
      .catch((errorInfo) => {
        // For converting to draft, only require title
        const titleErrors = errorInfo.errorFields?.filter(
          (field: any) => field.name[0] === "title"
        );
        if (titleErrors && titleErrors.length > 0) {
          message.error("Please enter a title to save as draft");
          return;
        }

        const values = form.getFieldsValue();
        if (!values.title || values.title.trim() === "") {
          message.error("Please enter a title to save as draft");
          return;
        }

        handleSubmit(values, PostStatus.Draft);
      });
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
          {/* Status indicator */}
          <div className="edit-blog-page__status-section">
            <Space align="center">
              <Typography.Text strong>Current Status:</Typography.Text>
              <Tag
                color={
                  blogPost.status === PostStatus.Published ? "green" : "orange"
                }
                icon={
                  blogPost.status === PostStatus.Published ? (
                    <SendOutlined />
                  ) : (
                    <FileTextOutlined />
                  )
                }
              >
                {blogPost.status === PostStatus.Published
                  ? "Published"
                  : "Draft"}
              </Tag>
            </Space>

            {blogPost.status === PostStatus.Draft && (
              <Alert
                message="This post is currently a draft"
                description="Only you can see this post. Publish it to make it visible to others."
                type="info"
                showIcon
                style={{ marginTop: 12 }}
              />
            )}
          </div>

          <Form
            form={form}
            layout="vertical"
            onFinish={handleSaveChanges}
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
              label="Featured Image"
              name="featuredImage"
              help="Upload an image from your computer or enter a URL below"
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
              help="Content is required for publishing, but optional for drafts"
            >
              <MarkdownEditor
                placeholder="Write your blog post content here..."
                rows={12}
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
              <Space size="middle" wrap>
                <Button
                  type="default"
                  onClick={() =>
                    navigate(
                      blogPost.status === PostStatus.Draft
                        ? "drafts"
                        : "blog-view",
                      undefined,
                      blogId
                    )
                  }
                  disabled={loading}
                >
                  Cancel
                </Button>

                <Button
                  type="default"
                  htmlType="submit"
                  icon={<SaveOutlined />}
                  loading={loading && actionType === "save"}
                  disabled={loading}
                  className="edit-blog-page__save-button"
                >
                  Save Changes
                </Button>

                {blogPost.status === PostStatus.Draft ? (
                  <Button
                    type="primary"
                    icon={<SendOutlined />}
                    loading={loading && actionType === "publish"}
                    disabled={loading}
                    onClick={handlePublish}
                    className="edit-blog-page__publish-button"
                  >
                    Publish
                  </Button>
                ) : (
                  <Button
                    type="default"
                    icon={<FileTextOutlined />}
                    loading={loading && actionType === "draft"}
                    disabled={loading}
                    onClick={handleConvertToDraft}
                    className="edit-blog-page__draft-button"
                  >
                    Convert to Draft
                  </Button>
                )}
              </Space>
            </Form.Item>
          </Form>
        </Card>
      </div>
    </div>
  );
};

export default EditBlogPage;
