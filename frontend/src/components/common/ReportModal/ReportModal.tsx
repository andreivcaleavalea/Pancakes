import React, { useState } from "react";
import { Modal, Form, Select, Input, Button, Typography, message } from "antd";
import { ExclamationCircleOutlined } from "@ant-design/icons";
import { reportApi } from "@/services/reportApi";
import type { CreateReportDto } from "@/types/report";
import {
  ReportReason,
  ReportContentType,
  REPORT_REASON_LABELS,
} from "@/types/report";
import "./ReportModal.scss";

const { TextArea } = Input;
const { Text } = Typography;

interface ReportModalProps {
  visible: boolean;
  onClose: () => void;
  contentType: ReportContentType;
  contentId: string;
  contentTitle?: string;
}

export const ReportModal: React.FC<ReportModalProps> = ({
  visible,
  onClose,
  contentType,
  contentId,
  contentTitle,
}) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (values: {
    reason: ReportReason;
    description?: string;
  }) => {
    try {
      setLoading(true);

      const reportData: CreateReportDto = {
        contentType,
        contentId,
        reason: values.reason,
        description: values.description,
      };

      await reportApi.create(reportData);

      message.success(
        "Report submitted successfully. We'll review it shortly."
      );
      form.resetFields();
      onClose();
    } catch (error: any) {
      console.error("Error submitting report:", error);

      if (error.response?.status === 400) {
        message.error(
          error.response.data.message ||
            "Unable to submit report. You may have already reported this content."
        );
      } else {
        message.error("Failed to submit report. Please try again.");
      }
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    form.resetFields();
    onClose();
  };

  const contentTypeLabel =
    contentType === ReportContentType.BlogPost ? "blog post" : "comment";

  return (
    <Modal
      title={
        <div className="report-modal__title">
          <ExclamationCircleOutlined
            style={{ color: "#ff4d4f", marginRight: 8 }}
          />
          Report {contentTypeLabel}
        </div>
      }
      open={visible}
      onCancel={handleCancel}
      footer={null}
      width={500}
      className="report-modal"
    >
      <div className="report-modal__content">
        <Text className="report-modal__description">
          Help us maintain a safe community by reporting content that violates
          our guidelines.
          {contentTitle && (
            <>
              <br />
              <strong>Reporting:</strong> {contentTitle}
            </>
          )}
        </Text>

        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          className="report-modal__form"
        >
          <Form.Item
            name="reason"
            label="Reason for reporting"
            rules={[
              {
                required: true,
                message: "Please select a reason for reporting",
              },
            ]}
          >
            <Select placeholder="Select a reason" size="large">
              {Object.entries(REPORT_REASON_LABELS).map(([value, label]) => (
                <Select.Option key={value} value={parseInt(value)}>
                  {label}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="description"
            label="Additional details (optional)"
            help="Provide any additional context that might help us understand the issue"
          >
            <TextArea
              rows={4}
              placeholder="Describe the issue in more detail..."
              maxLength={1000}
              showCount
              className="report-modal__textarea"
            />
          </Form.Item>

          <div className="report-modal__actions">
            <Button onClick={handleCancel} style={{ marginRight: 8 }}>
              Cancel
            </Button>
            <Button type="primary" danger htmlType="submit" loading={loading}>
              Submit Report
            </Button>
          </div>
        </Form>
      </div>
    </Modal>
  );
};
