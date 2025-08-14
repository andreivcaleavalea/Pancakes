import React, { useState } from "react";
import { Upload, Button, message, Image, Spin, Modal } from "antd";
import { UploadOutlined, DeleteOutlined, EyeOutlined } from "@ant-design/icons";
import type { UploadFile, UploadProps } from "antd";
import { blogImageApi } from "@/services/blogImageApi";
import "./ImageUpload.scss";

interface ImageUploadProps {
  value?: string;
  onChange?: (value?: string) => void;
  maxCount?: number;
  disabled?: boolean;
  placeholder?: string;
}

const ImageUpload: React.FC<ImageUploadProps> = ({
  value,
  onChange,
  maxCount = 1,
  disabled = false,
  placeholder = "Click to upload image",
}) => {
  const [loading, setLoading] = useState(false);
  const [previewVisible, setPreviewVisible] = useState(false);
  const [previewImage, setPreviewImage] = useState("");

  const handleUpload = async (file: File): Promise<boolean> => {
    try {
      setLoading(true);
      const formData = new FormData();
      formData.append("image", file);

      const response = await blogImageApi.upload(formData);

      if (response.imageUrl) {
        onChange?.(response.imageUrl);
        message.success("Image uploaded successfully!");
      }

      return true;
    } catch (error) {
      console.error("Upload error:", error);
      message.error("Failed to upload image. Please try again.");
      return false;
    } finally {
      setLoading(false);
    }
  };

  const handleRemove = async () => {
    try {
      if (value) {
        // Extract filename from URL for deletion (only for uploaded images)
        const filename = value.split("/").pop();
        if (filename && value.includes("/assets/blog-images/")) {
          await blogImageApi.delete(filename);
        }
        onChange?.(undefined);
        message.success("Image removed successfully!");
      }
    } catch (error) {
      console.error("Delete error:", error);
      message.error("Failed to remove image.");
    }
  };

  const handlePreview = () => {
    if (value) {
      setPreviewImage(value);
      setPreviewVisible(true);
    }
  };

  const uploadProps: UploadProps = {
    name: "image",
    customRequest: async ({ file, onSuccess, onError }) => {
      try {
        const success = await handleUpload(file as File);
        if (success) {
          onSuccess?.("ok");
        } else {
          onError?.(new Error("Upload failed"));
        }
      } catch (error) {
        onError?.(error as Error);
      }
    },
    beforeUpload: (file) => {
      const isImage = file.type.startsWith("image/");
      const isLt10M = file.size / 1024 / 1024 < 10;

      if (!isImage) {
        message.error("You can only upload image files!");
        return false;
      }

      if (!isLt10M) {
        message.error("Image must be smaller than 10MB!");
        return false;
      }

      return true;
    },
    showUploadList: false,
    disabled: disabled || loading,
    maxCount,
  };

  return (
    <div className="image-upload">
      {value ? (
        <div className="image-upload__preview">
          <div className="image-upload__preview-container">
            <Image
              src={value}
              alt="Uploaded image"
              className="image-upload__image"
              preview={false}
            />
            <div className="image-upload__overlay">
              <Button
                icon={<EyeOutlined />}
                size="small"
                onClick={handlePreview}
                className="image-upload__action-btn"
              />
              <Button
                icon={<DeleteOutlined />}
                size="small"
                danger
                onClick={handleRemove}
                className="image-upload__action-btn"
                disabled={disabled}
              />
            </div>
          </div>
          <div className="image-upload__actions">
            <Upload {...uploadProps}>
              <Button
                icon={<UploadOutlined />}
                loading={loading}
                disabled={disabled}
                size="small"
              >
                Replace
              </Button>
            </Upload>
          </div>
        </div>
      ) : (
        <div className="image-upload__empty">
          <Upload {...uploadProps}>
            <Button
              icon={<UploadOutlined />}
              loading={loading}
              disabled={disabled}
              className="image-upload__upload-btn"
            >
              {loading ? "Uploading..." : placeholder}
            </Button>
          </Upload>
        </div>
      )}

      <Modal
        open={previewVisible}
        footer={null}
        onCancel={() => setPreviewVisible(false)}
        centered
      >
        <Image src={previewImage} alt="Preview" style={{ width: "100%" }} />
      </Modal>

      {loading && (
        <div className="image-upload__loading">
          <Spin size="small" />
          <span>Uploading...</span>
        </div>
      )}
    </div>
  );
};

export default ImageUpload;
