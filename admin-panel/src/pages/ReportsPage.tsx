import React, { useState, useEffect } from "react";
import {
  Table,
  Card,
  Typography,
  Space,
  Button,
  Tag,
  Select,
  Modal,
  Form,
  Input,
  Checkbox,
  message,
  Popconfirm,
  Row,
  Col,
  Statistic,
  Tooltip,
} from "antd";
import {
  EyeOutlined,
  CheckOutlined,
  CloseOutlined,
  DeleteOutlined,
  UserDeleteOutlined,
  FileTextOutlined,
  FlagOutlined,
} from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import { reportApi } from "@/services/reportApi";
import { adminActionsApi } from "@/services/adminActionsApi";
import {
  ReportDto,
  UpdateReportDto,
  ReportStatus,
  ReportReason,
  ReportContentType,
  REPORT_REASON_LABELS,
  REPORT_STATUS_LABELS,
  REPORT_CONTENT_TYPE_LABELS,
  ReportStats,
} from "@/types/report";

const { Title, Text } = Typography;
const { TextArea } = Input;

const ReportsPage: React.FC = () => {
  const [reports, setReports] = useState<ReportDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [stats, setStats] = useState<ReportStats>({
    totalReports: 0,
    pendingReports: 0,
  });
  const [selectedStatus, setSelectedStatus] = useState<
    ReportStatus | undefined
  >();
  const [selectedReport, setSelectedReport] = useState<ReportDto | null>(null);
  const [reviewModalVisible, setReviewModalVisible] = useState(false);
  const [detailModalVisible, setDetailModalVisible] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);

  const [form] = Form.useForm();

  // Load reports and stats
  const loadReports = async () => {
    try {
      setLoading(true);
      const [reportsData, statsData] = await Promise.all([
        reportApi.getAll(1, 100, selectedStatus),
        reportApi.getStats(),
      ]);
      setReports(reportsData);
      setStats(statsData);
    } catch (error) {
      console.error("Error loading reports:", error);
      message.error("Failed to load reports");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadReports();
  }, [selectedStatus]);

  // Handle report review/resolution
  const handleResolveReport = async (values: any) => {
    if (!selectedReport) return;

    try {
      setActionLoading(true);

      const actions: string[] = [];

      // Perform admin actions if requested
      if (values.userBanned) {
        try {
          await adminActionsApi.banUser({
            userId: selectedReport.reportedUserId,
            reason: `Banned due to report: ${
              REPORT_REASON_LABELS[selectedReport.reason]
            }. ${values.adminNotes || ""}`.trim(),
          });
          actions.push("User banned");
        } catch (error: any) {
          console.error("Error banning user:", error);
          message.error(`Failed to ban user: ${error.message}`);
          return; // Don't continue if ban failed
        }
      }

      if (values.contentRemoved) {
        try {
          if (selectedReport.contentType === ReportContentType.BlogPost) {
            await adminActionsApi.deleteBlogPost(
              selectedReport.contentId,
              "Content reported and removed by admin"
            );
            actions.push("Blog post removed");
          } else {
            await adminActionsApi.deleteComment(selectedReport.contentId);
            actions.push("Comment removed");
          }
        } catch (error: any) {
          console.error("Error removing content:", error);
          message.error(`Failed to remove content: ${error.message}`);
          return; // Don't continue if content removal failed
        }
      }

      // Update the report with the actions taken
      const updateData: UpdateReportDto = {
        status: values.status,
        adminNotes: values.adminNotes,
        userBanned: values.userBanned || false,
        contentRemoved: values.contentRemoved || false,
      };

      await reportApi.update(selectedReport.id, updateData);

      const successMessage =
        actions.length > 0
          ? `Report updated successfully. Actions taken: ${actions.join(", ")}`
          : "Report updated successfully";

      message.success(successMessage);
      setReviewModalVisible(false);
      form.resetFields();
      setSelectedReport(null);
      loadReports();
    } catch (error: any) {
      console.error("Error updating report:", error);
      message.error(error.message || "Failed to update report");
    } finally {
      setActionLoading(false);
    }
  };

  // Quick actions
  const handleQuickResolve = async (reportId: string) => {
    try {
      await reportApi.update(reportId, { status: ReportStatus.Resolved });
      message.success("Report marked as resolved");
      loadReports();
    } catch (error: any) {
      message.error(error.message || "Failed to resolve report");
    }
  };

  const handleQuickDismiss = async (reportId: string) => {
    try {
      await reportApi.update(reportId, { status: ReportStatus.Dismissed });
      message.success("Report dismissed");
      loadReports();
    } catch (error: any) {
      message.error(error.message || "Failed to dismiss report");
    }
  };

  const handleDeleteReport = async (reportId: string) => {
    try {
      await reportApi.delete(reportId);
      message.success("Report deleted");
      loadReports();
    } catch (error: any) {
      message.error(error.message || "Failed to delete report");
    }
  };

  // Open modals
  const openReviewModal = (report: ReportDto) => {
    setSelectedReport(report);
    form.setFieldsValue({
      status: ReportStatus.UnderReview,
      adminNotes: "",
      userBanned: false,
      contentRemoved: false,
    });
    setReviewModalVisible(true);
  };

  const openDetailModal = (report: ReportDto) => {
    setSelectedReport(report);
    setDetailModalVisible(true);
  };

  // Get status color
  const getStatusColor = (status: ReportStatus): string => {
    switch (status) {
      case ReportStatus.Pending:
        return "orange";
      case ReportStatus.UnderReview:
        return "blue";
      case ReportStatus.Resolved:
        return "green";
      case ReportStatus.Dismissed:
        return "red";
      default:
        return "default";
    }
  };

  // Get reason color
  const getReasonColor = (reason: ReportReason): string => {
    switch (reason) {
      case ReportReason.Violence:
      case ReportReason.HateSpeech:
        return "red";
      case ReportReason.SexualContent:
        return "magenta";
      case ReportReason.Harassment:
        return "volcano";
      case ReportReason.Spam:
        return "orange";
      default:
        return "blue";
    }
  };

  const columns: ColumnsType<ReportDto> = [
    {
      title: "Content",
      key: "content",
      render: (_, record) => (
        <Space direction="vertical" size="small">
          <Space>
            <Tag
              color={
                record.contentType === ReportContentType.BlogPost
                  ? "blue"
                  : "green"
              }
            >
              {REPORT_CONTENT_TYPE_LABELS[record.contentType]}
            </Tag>
            <Text strong>{record.contentTitle}</Text>
          </Space>
          <Text type="secondary" ellipsis style={{ maxWidth: 200 }}>
            {record.contentExcerpt}
          </Text>
        </Space>
      ),
    },
    {
      title: "Reason",
      dataIndex: "reason",
      key: "reason",
      render: (reason: ReportReason) => (
        <Tag color={getReasonColor(reason)}>{REPORT_REASON_LABELS[reason]}</Tag>
      ),
    },
    {
      title: "Reporter",
      key: "reporter",
      render: (_, record) => (
        <Space direction="vertical" size="small">
          <Text strong>{record.reporterName}</Text>
          <Text type="secondary" copyable style={{ fontSize: 12 }}>
            {record.reporterId}
          </Text>
        </Space>
      ),
    },
    {
      title: "Reported User",
      key: "reportedUser",
      render: (_, record) => (
        <Space direction="vertical" size="small">
          <Text strong>{record.reportedUserName || "Unknown"}</Text>
          <Text type="secondary" copyable style={{ fontSize: 12 }}>
            {record.reportedUserId}
          </Text>
        </Space>
      ),
    },
    {
      title: "Status",
      dataIndex: "status",
      key: "status",
      render: (status: ReportStatus) => (
        <Tag color={getStatusColor(status)}>{REPORT_STATUS_LABELS[status]}</Tag>
      ),
    },
    {
      title: "Actions Taken",
      key: "actionsTaken",
      render: (_, record) => (
        <Space>
          {record.userBanned && (
            <Tag color="red" icon={<UserDeleteOutlined />}>
              User Banned
            </Tag>
          )}
          {record.contentRemoved && (
            <Tag color="orange" icon={<FileTextOutlined />}>
              Content Removed
            </Tag>
          )}
        </Space>
      ),
    },
    {
      title: "Date",
      dataIndex: "createdAt",
      key: "createdAt",
      render: (date: string) => new Date(date).toLocaleDateString(),
      sorter: (a, b) =>
        new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime(),
    },
    {
      title: "Actions",
      key: "actions",
      render: (_, record) => (
        <Space>
          <Tooltip title="View Details">
            <Button
              type="text"
              icon={<EyeOutlined />}
              onClick={() => openDetailModal(record)}
            />
          </Tooltip>

          {record.status === ReportStatus.Pending && (
            <>
              <Tooltip title="Review Report">
                <Button
                  type="primary"
                  size="small"
                  onClick={() => openReviewModal(record)}
                >
                  Review
                </Button>
              </Tooltip>

              <Tooltip title="Quick Resolve">
                <Button
                  type="text"
                  icon={<CheckOutlined />}
                  onClick={() => handleQuickResolve(record.id)}
                />
              </Tooltip>

              <Tooltip title="Quick Dismiss">
                <Button
                  type="text"
                  icon={<CloseOutlined />}
                  onClick={() => handleQuickDismiss(record.id)}
                />
              </Tooltip>
            </>
          )}

          <Popconfirm
            title="Delete Report"
            description="Are you sure you want to delete this report?"
            onConfirm={() => handleDeleteReport(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Tooltip title="Delete Report">
              <Button type="text" danger icon={<DeleteOutlined />} />
            </Tooltip>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: "24px" }}>
      <Space direction="vertical" style={{ width: "100%" }} size="large">
        {/* Page Header */}
        <div>
          <Title level={2}>
            <FlagOutlined /> Content Reports
          </Title>
          <Text type="secondary">
            Review and manage user-submitted content reports
          </Text>
        </div>

        {/* Statistics */}
        <Row gutter={16}>
          <Col span={8}>
            <Card>
              <Statistic
                title="Total Reports"
                value={stats.totalReports}
                prefix={<FlagOutlined />}
              />
            </Card>
          </Col>
          <Col span={8}>
            <Card>
              <Statistic
                title="Pending Review"
                value={stats.pendingReports}
                prefix={<FlagOutlined />}
                valueStyle={{ color: "#fa8c16" }}
              />
            </Card>
          </Col>
          <Col span={8}>
            <Card>
              <Statistic
                title="Resolution Rate"
                value={
                  stats.totalReports > 0
                    ? Math.round(
                        ((stats.totalReports - stats.pendingReports) /
                          stats.totalReports) *
                          100
                      )
                    : 0
                }
                suffix="%"
                prefix={<CheckOutlined />}
                valueStyle={{ color: "#52c41a" }}
              />
            </Card>
          </Col>
        </Row>

        {/* Filters */}
        <Card>
          <Space>
            <Text strong>Filter by Status:</Text>
            <Select
              placeholder="All Statuses"
              style={{ width: 200 }}
              allowClear
              value={selectedStatus}
              onChange={setSelectedStatus}
            >
              {Object.entries(REPORT_STATUS_LABELS).map(([value, label]) => (
                <Select.Option key={value} value={parseInt(value)}>
                  {label}
                </Select.Option>
              ))}
            </Select>
          </Space>
        </Card>

        {/* Reports Table */}
        <Card>
          <Table
            columns={columns}
            dataSource={reports}
            rowKey="id"
            loading={loading}
            pagination={{
              pageSize: 20,
              showSizeChanger: true,
              showQuickJumper: true,
              showTotal: (total, range) =>
                `${range[0]}-${range[1]} of ${total} reports`,
            }}
            scroll={{ x: 1200 }}
          />
        </Card>
      </Space>

      {/* Review Modal */}
      <Modal
        title="Review Report"
        open={reviewModalVisible}
        onCancel={() => {
          setReviewModalVisible(false);
          form.resetFields();
          setSelectedReport(null);
        }}
        footer={null}
        width={600}
      >
        {selectedReport && (
          <Form form={form} layout="vertical" onFinish={handleResolveReport}>
            <Space direction="vertical" style={{ width: "100%" }} size="middle">
              {/* Report Details */}
              <Card size="small" title="Report Details">
                <Space direction="vertical" style={{ width: "100%" }}>
                  <div>
                    <Text strong>Content: </Text>
                    <Tag
                      color={
                        selectedReport.contentType ===
                        ReportContentType.BlogPost
                          ? "blue"
                          : "green"
                      }
                    >
                      {REPORT_CONTENT_TYPE_LABELS[selectedReport.contentType]}
                    </Tag>
                    <Text>{selectedReport.contentTitle}</Text>
                  </div>
                  <div>
                    <Text strong>Reason: </Text>
                    <Tag color={getReasonColor(selectedReport.reason)}>
                      {REPORT_REASON_LABELS[selectedReport.reason]}
                    </Tag>
                  </div>
                  {selectedReport.description && (
                    <div>
                      <Text strong>Description: </Text>
                      <Text>{selectedReport.description}</Text>
                    </div>
                  )}
                  <div>
                    <Text strong>Reporter: </Text>
                    <Text>{selectedReport.reporterName}</Text>
                  </div>
                  <div>
                    <Text strong>Reported User: </Text>
                    <Text>{selectedReport.reportedUserName}</Text>
                  </div>
                </Space>
              </Card>

              {/* Admin Actions */}
              <Form.Item
                name="status"
                label="Resolution Status"
                rules={[{ required: true, message: "Please select a status" }]}
              >
                <Select>
                  <Select.Option value={ReportStatus.UnderReview}>
                    Under Review
                  </Select.Option>
                  <Select.Option value={ReportStatus.Resolved}>
                    Resolved
                  </Select.Option>
                  <Select.Option value={ReportStatus.Dismissed}>
                    Dismissed
                  </Select.Option>
                </Select>
              </Form.Item>

              <Form.Item name="adminNotes" label="Admin Notes">
                <TextArea
                  rows={4}
                  placeholder="Add any notes about your decision..."
                />
              </Form.Item>

              <Form.Item name="userBanned" valuePropName="checked">
                <Checkbox>Ban the reported user</Checkbox>
              </Form.Item>

              <Form.Item name="contentRemoved" valuePropName="checked">
                <Checkbox>Remove the reported content</Checkbox>
              </Form.Item>

              <Form.Item>
                <Space>
                  <Button
                    onClick={() => {
                      setReviewModalVisible(false);
                      form.resetFields();
                      setSelectedReport(null);
                    }}
                  >
                    Cancel
                  </Button>
                  <Button
                    type="primary"
                    htmlType="submit"
                    loading={actionLoading}
                  >
                    Update Report
                  </Button>
                </Space>
              </Form.Item>
            </Space>
          </Form>
        )}
      </Modal>

      {/* Detail Modal */}
      <Modal
        title="Report Details"
        open={detailModalVisible}
        onCancel={() => {
          setDetailModalVisible(false);
          setSelectedReport(null);
        }}
        footer={[
          <Button
            key="close"
            onClick={() => {
              setDetailModalVisible(false);
              setSelectedReport(null);
            }}
          >
            Close
          </Button>,
        ]}
        width={700}
      >
        {selectedReport && (
          <Space direction="vertical" style={{ width: "100%" }} size="large">
            <Card title="Content Information">
              <Space direction="vertical" style={{ width: "100%" }}>
                <div>
                  <Text strong>Type: </Text>
                  <Tag
                    color={
                      selectedReport.contentType === ReportContentType.BlogPost
                        ? "blue"
                        : "green"
                    }
                  >
                    {REPORT_CONTENT_TYPE_LABELS[selectedReport.contentType]}
                  </Tag>
                </div>
                <div>
                  <Text strong>Title: </Text>
                  <Text>{selectedReport.contentTitle}</Text>
                </div>
                <div>
                  <Text strong>Excerpt: </Text>
                  <Text>{selectedReport.contentExcerpt}</Text>
                </div>
                <div>
                  <Text strong>Content ID: </Text>
                  <Text code copyable>
                    {selectedReport.contentId}
                  </Text>
                </div>
              </Space>
            </Card>

            <Card title="Report Information">
              <Space direction="vertical" style={{ width: "100%" }}>
                <div>
                  <Text strong>Reason: </Text>
                  <Tag color={getReasonColor(selectedReport.reason)}>
                    {REPORT_REASON_LABELS[selectedReport.reason]}
                  </Tag>
                </div>
                {selectedReport.description && (
                  <div>
                    <Text strong>Description: </Text>
                    <Text>{selectedReport.description}</Text>
                  </div>
                )}
                <div>
                  <Text strong>Status: </Text>
                  <Tag color={getStatusColor(selectedReport.status)}>
                    {REPORT_STATUS_LABELS[selectedReport.status]}
                  </Tag>
                </div>
                <div>
                  <Text strong>Reported At: </Text>
                  <Text>
                    {new Date(selectedReport.createdAt).toLocaleString()}
                  </Text>
                </div>
              </Space>
            </Card>

            <Card title="User Information">
              <Space direction="vertical" style={{ width: "100%" }}>
                <div>
                  <Text strong>Reporter: </Text>
                  <Text>{selectedReport.reporterName}</Text>
                  <Text code copyable style={{ marginLeft: 8 }}>
                    {selectedReport.reporterId}
                  </Text>
                </div>
                <div>
                  <Text strong>Reported User: </Text>
                  <Text>{selectedReport.reportedUserName}</Text>
                  <Text code copyable style={{ marginLeft: 8 }}>
                    {selectedReport.reportedUserId}
                  </Text>
                </div>
              </Space>
            </Card>

            {(selectedReport.reviewedBy || selectedReport.adminNotes) && (
              <Card title="Admin Review">
                <Space direction="vertical" style={{ width: "100%" }}>
                  {selectedReport.reviewedBy && (
                    <div>
                      <Text strong>Reviewed By: </Text>
                      <Text>{selectedReport.reviewedBy}</Text>
                    </div>
                  )}
                  {selectedReport.reviewedAt && (
                    <div>
                      <Text strong>Reviewed At: </Text>
                      <Text>
                        {new Date(selectedReport.reviewedAt).toLocaleString()}
                      </Text>
                    </div>
                  )}
                  {selectedReport.adminNotes && (
                    <div>
                      <Text strong>Admin Notes: </Text>
                      <Text>{selectedReport.adminNotes}</Text>
                    </div>
                  )}
                  {(selectedReport.userBanned ||
                    selectedReport.contentRemoved) && (
                    <div>
                      <Text strong>Actions Taken: </Text>
                      <Space>
                        {selectedReport.userBanned && (
                          <Tag color="red" icon={<UserDeleteOutlined />}>
                            User Banned
                          </Tag>
                        )}
                        {selectedReport.contentRemoved && (
                          <Tag color="orange" icon={<FileTextOutlined />}>
                            Content Removed
                          </Tag>
                        )}
                      </Space>
                    </div>
                  )}
                </Space>
              </Card>
            )}
          </Space>
        )}
      </Modal>
    </div>
  );
};

export default ReportsPage;
