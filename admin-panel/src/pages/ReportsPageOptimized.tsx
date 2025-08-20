import React, { useState, useMemo, useCallback, memo } from "react";
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
  ClockCircleOutlined,
} from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import { useReports } from "@/hooks/useReports";
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
} from "@/types/report";

const { Title, Text } = Typography;
const { TextArea } = Input;

// Memoized status tag component for better performance
const StatusTag = memo<{ status: ReportStatus }>(({ status }) => {
  const config = useMemo(() => {
    const statusConfig = {
      [ReportStatus.Pending]: {
        color: "orange",
        label: REPORT_STATUS_LABELS[ReportStatus.Pending],
      },
      [ReportStatus.Resolved]: {
        color: "green",
        label: REPORT_STATUS_LABELS[ReportStatus.Resolved],
      },
      [ReportStatus.Dismissed]: {
        color: "red",
        label: REPORT_STATUS_LABELS[ReportStatus.Dismissed],
      },
    };
    return statusConfig[status];
  }, [status]);

  return <Tag color={config.color}>{config.label}</Tag>;
});

StatusTag.displayName = "StatusTag";

// Memoized reason tag component
const ReasonTag = memo<{ reason: ReportReason }>(({ reason }) => (
  <Tag>{REPORT_REASON_LABELS[reason]}</Tag>
));

ReasonTag.displayName = "ReasonTag";

// Memoized content type tag component
const ContentTypeTag = memo<{ contentType: ReportContentType }>(
  ({ contentType }) => {
    const config = useMemo(
      () => ({
        color: contentType === ReportContentType.BlogPost ? "blue" : "purple",
        label: REPORT_CONTENT_TYPE_LABELS[contentType],
      }),
      [contentType]
    );

    return <Tag color={config.color}>{config.label}</Tag>;
  }
);

ContentTypeTag.displayName = "ContentTypeTag";

// Memoized action buttons component
const ActionButtons = memo<{
  report: ReportDto;
  onView: (report: ReportDto) => void;
  onReview: (report: ReportDto) => void;
  onDelete: (reportId: string) => void;
  loading: boolean;
}>(({ report, onView, onReview, onDelete, loading }) => {
  const handleView = useCallback(() => onView(report), [onView, report]);
  const handleReview = useCallback(() => onReview(report), [onReview, report]);
  const handleDelete = useCallback(
    () => onDelete(report.id),
    [onDelete, report.id]
  );

  return (
    <Space size="small">
      <Tooltip title="View Details">
        <Button
          icon={<EyeOutlined />}
          size="small"
          onClick={handleView}
          disabled={loading}
        />
      </Tooltip>
      {report.status === ReportStatus.Pending && (
        <Tooltip title="Review Report">
          <Button
            icon={<CheckOutlined />}
            size="small"
            type="primary"
            onClick={handleReview}
            disabled={loading}
          />
        </Tooltip>
      )}
      <Tooltip title="Delete Report">
        <Popconfirm
          title="Are you sure you want to delete this report?"
          onConfirm={handleDelete}
          disabled={loading}
        >
          <Button
            icon={<DeleteOutlined />}
            size="small"
            danger
            disabled={loading}
          />
        </Popconfirm>
      </Tooltip>
    </Space>
  );
});

ActionButtons.displayName = "ActionButtons";

const ReportsPage: React.FC = () => {
  // Use the optimized hook
  const {
    reports,
    stats,
    loading,
    actionLoading,
    error,
    pagination,
    selectedStatus,
    filterByStatus,
    changePage,
    refresh,
    updateReport,
    deleteReport,
  } = useReports({
    initialPageSize: 20, // Optimized page size
    debounceMs: 300, // Debounce filter changes
    enableCaching: true, // Enable caching
  });

  const [selectedReport, setSelectedReport] = useState<ReportDto | null>(null);
  const [reviewModalVisible, setReviewModalVisible] = useState(false);
  const [detailModalVisible, setDetailModalVisible] = useState(false);

  const [form] = Form.useForm();

  // Memoized handlers to prevent unnecessary re-renders
  const handleViewReport = useCallback((report: ReportDto) => {
    setSelectedReport(report);
    setDetailModalVisible(true);
  }, []);

  const handleReviewReport = useCallback(
    (report: ReportDto) => {
      setSelectedReport(report);
      setReviewModalVisible(true);
      form.resetFields();
    },
    [form]
  );

  const handleCloseModals = useCallback(() => {
    setReviewModalVisible(false);
    setDetailModalVisible(false);
    setSelectedReport(null);
    form.resetFields();
  }, [form]);

  // Handle report review/resolution
  const handleResolveReport = useCallback(
    async (values: any) => {
      if (!selectedReport) return;

      try {
        const actions: string[] = [];
        const updateData: UpdateReportDto = {
          status: ReportStatus.Resolved,
          adminNotes: values.adminNotes,
          userBanned: !!values.userBanned,
          contentRemoved: !!values.contentRemoved,
        };

        // Perform admin actions if requested
        if (values.userBanned) {
          try {
            await adminActionsApi.banUser({
              userId: selectedReport.reportedUserId,
              reason:
                values.banReason || "Content reported and user banned by admin",
              expiresAt: values.banExpiry,
            });
            actions.push("User banned");
          } catch (error: any) {
            console.error("Error banning user:", error);
            message.error(`Failed to ban user: ${error.message}`);
            return;
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
            return;
          }
        }

        // Update the report
        await updateReport(selectedReport.id, updateData);

        const actionMessage =
          actions.length > 0
            ? `Report resolved. Actions taken: ${actions.join(", ")}`
            : "Report resolved successfully";

        message.success(actionMessage);
        handleCloseModals();
      } catch (error: any) {
        console.error("Error resolving report:", error);
        message.error(error?.message || "Failed to resolve report");
      }
    },
    [selectedReport, updateReport, handleCloseModals]
  );

  const handleDismissReport = useCallback(
    async (values: any) => {
      if (!selectedReport) return;

      try {
        const updateData: UpdateReportDto = {
          status: ReportStatus.Dismissed,
          adminNotes: values.adminNotes,
          userBanned: false,
          contentRemoved: false,
        };

        await updateReport(selectedReport.id, updateData);
        message.success("Report dismissed successfully");
        handleCloseModals();
      } catch (error: any) {
        console.error("Error dismissing report:", error);
        message.error(error?.message || "Failed to dismiss report");
      }
    },
    [selectedReport, updateReport, handleCloseModals]
  );

  // Memoized table columns for performance
  const columns: ColumnsType<ReportDto> = useMemo(
    () => [
      {
        title: "ID",
        dataIndex: "id",
        key: "id",
        width: 100,
        render: (id: string) => (
          <Text code style={{ fontSize: "12px" }}>
            {id.slice(0, 8)}...
          </Text>
        ),
      },
      {
        title: "Content Type",
        dataIndex: "contentType",
        key: "contentType",
        width: 120,
        render: (contentType: ReportContentType) => (
          <ContentTypeTag contentType={contentType} />
        ),
      },
      {
        title: "Content Title",
        dataIndex: "contentTitle",
        key: "contentTitle",
        width: 200,
        render: (title: string) => (
          <Tooltip title={title}>
            <Text ellipsis style={{ maxWidth: 180, display: "block" }}>
              {title}
            </Text>
          </Tooltip>
        ),
      },
      {
        title: "Reason",
        dataIndex: "reason",
        key: "reason",
        width: 120,
        render: (reason: ReportReason) => <ReasonTag reason={reason} />,
      },
      {
        title: "Reporter",
        dataIndex: "reporterName",
        key: "reporterName",
        width: 120,
        render: (name: string) => <Text>{name}</Text>,
      },
      {
        title: "Reported User",
        dataIndex: "reportedUserName",
        key: "reportedUserName",
        width: 120,
        render: (name: string) => <Text>{name}</Text>,
      },
      {
        title: "Status",
        dataIndex: "status",
        key: "status",
        width: 100,
        render: (status: ReportStatus) => <StatusTag status={status} />,
      },
      {
        title: "Created",
        dataIndex: "createdAt",
        key: "createdAt",
        width: 130,
        render: (date: string) => (
          <Text style={{ fontSize: "12px" }}>
            {new Date(date).toLocaleDateString()}
          </Text>
        ),
      },
      {
        title: "Actions",
        key: "actions",
        width: 150,
        fixed: "right" as const,
        render: (_, record: ReportDto) => (
          <ActionButtons
            report={record}
            onView={handleViewReport}
            onReview={handleReviewReport}
            onDelete={deleteReport}
            loading={actionLoading}
          />
        ),
      },
    ],
    [handleViewReport, handleReviewReport, deleteReport, actionLoading]
  );

  // Memoized status filter options
  const statusFilterOptions = useMemo(
    () =>
      Object.entries(REPORT_STATUS_LABELS).map(([value, label]) => ({
        value: parseInt(value),
        label,
      })),
    []
  );

  if (error) {
    return (
      <Card>
        <div style={{ textAlign: "center", padding: "40px" }}>
          <Text type="danger">{error}</Text>
          <br />
          <Button type="primary" onClick={refresh} style={{ marginTop: 16 }}>
            Retry
          </Button>
        </div>
      </Card>
    );
  }

  return (
    <div style={{ padding: "24px" }}>
      <Space direction="vertical" size="large" style={{ width: "100%" }}>
        {/* Header */}
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Title level={2} style={{ margin: 0 }}>
            <FlagOutlined style={{ marginRight: 8 }} />
            Content Reports
          </Title>
          <Button onClick={refresh} loading={loading}>
            Refresh
          </Button>
        </div>

        {/* Stats Cards */}
        <Row gutter={16}>
          <Col span={12}>
            <Card>
              <Statistic
                title="Total Reports"
                value={stats.totalReports}
                prefix={<FlagOutlined />}
              />
            </Card>
          </Col>
          <Col span={12}>
            <Card>
              <Statistic
                title="Pending Reports"
                value={stats.pendingReports}
                prefix={<ClockCircleOutlined />}
                valueStyle={{ color: "#fa8c16" }}
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
              onChange={filterByStatus}
              options={statusFilterOptions}
            />
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
              ...pagination,
              onChange: changePage,
              onShowSizeChange: changePage,
            }}
            scroll={{ x: 1200 }}
            size="small"
          />
        </Card>
      </Space>

      {/* Review Modal */}
      <Modal
        title="Review Report"
        open={reviewModalVisible}
        onCancel={handleCloseModals}
        footer={null}
        width={600}
      >
        {selectedReport && (
          <Form form={form} layout="vertical" onFinish={handleResolveReport}>
            <Space direction="vertical" style={{ width: "100%" }}>
              {/* Report Details */}
              <Card title="Report Details" size="small">
                <Space direction="vertical" style={{ width: "100%" }}>
                  <div>
                    <Text strong>Content: </Text>
                    <ContentTypeTag contentType={selectedReport.contentType} />
                    <Text style={{ marginLeft: 8 }}>
                      {selectedReport.contentTitle}
                    </Text>
                  </div>
                  <div>
                    <Text strong>Reason: </Text>
                    <ReasonTag reason={selectedReport.reason} />
                  </div>
                  <div>
                    <Text strong>Reporter: </Text>
                    <Text>{selectedReport.reporterName}</Text>
                  </div>
                  <div>
                    <Text strong>Reported User: </Text>
                    <Text>{selectedReport.reportedUserName}</Text>
                  </div>
                  {selectedReport.description && (
                    <div>
                      <Text strong>Description: </Text>
                      <Text>{selectedReport.description}</Text>
                    </div>
                  )}
                </Space>
              </Card>

              {/* Admin Actions */}
              <Card title="Actions" size="small">
                <Space direction="vertical" style={{ width: "100%" }}>
                  <Form.Item name="userBanned" valuePropName="checked">
                    <Checkbox>
                      <Text strong>Ban reported user</Text>
                    </Checkbox>
                  </Form.Item>

                  <Form.Item name="contentRemoved" valuePropName="checked">
                    <Checkbox>
                      <Text strong>Remove reported content</Text>
                    </Checkbox>
                  </Form.Item>

                  <Form.Item name="adminNotes" label="Admin Notes">
                    <TextArea
                      rows={3}
                      placeholder="Add notes about your decision..."
                    />
                  </Form.Item>
                </Space>
              </Card>

              {/* Action Buttons */}
              <div
                style={{ display: "flex", justifyContent: "flex-end", gap: 8 }}
              >
                <Button onClick={handleCloseModals}>Cancel</Button>
                <Button
                  onClick={() =>
                    form.validateFields().then(handleDismissReport)
                  }
                  loading={actionLoading}
                >
                  Dismiss
                </Button>
                <Button
                  type="primary"
                  htmlType="submit"
                  loading={actionLoading}
                >
                  Resolve
                </Button>
              </div>
            </Space>
          </Form>
        )}
      </Modal>

      {/* Detail Modal */}
      <Modal
        title="Report Details"
        open={detailModalVisible}
        onCancel={handleCloseModals}
        footer={[
          <Button key="close" onClick={handleCloseModals}>
            Close
          </Button>,
        ]}
        width={700}
      >
        {selectedReport && (
          <Space direction="vertical" style={{ width: "100%" }}>
            {/* Report Info */}
            <Card title="Report Information" size="small">
              <Row gutter={16}>
                <Col span={12}>
                  <Space direction="vertical" style={{ width: "100%" }}>
                    <div>
                      <Text strong>Report ID: </Text>
                      <Text code>{selectedReport.id}</Text>
                    </div>
                    <div>
                      <Text strong>Status: </Text>
                      <StatusTag status={selectedReport.status} />
                    </div>
                    <div>
                      <Text strong>Created: </Text>
                      <Text>
                        {new Date(selectedReport.createdAt).toLocaleString()}
                      </Text>
                    </div>
                  </Space>
                </Col>
                <Col span={12}>
                  <Space direction="vertical" style={{ width: "100%" }}>
                    <div>
                      <Text strong>Content Type: </Text>
                      <ContentTypeTag
                        contentType={selectedReport.contentType}
                      />
                    </div>
                    <div>
                      <Text strong>Reason: </Text>
                      <ReasonTag reason={selectedReport.reason} />
                    </div>
                    <div>
                      <Text strong>Reporter: </Text>
                      <Text>{selectedReport.reporterName}</Text>
                    </div>
                  </Space>
                </Col>
              </Row>
            </Card>

            {/* Content Details */}
            <Card title="Reported Content" size="small">
              <Space direction="vertical" style={{ width: "100%" }}>
                <div>
                  <Text strong>Title: </Text>
                  <Text>{selectedReport.contentTitle}</Text>
                </div>
                <div>
                  <Text strong>Author: </Text>
                  <Text>{selectedReport.reportedUserName}</Text>
                </div>
                {selectedReport.description && (
                  <div>
                    <Text strong>Report Description: </Text>
                    <div
                      style={{
                        background: "#f5f5f5",
                        padding: "8px",
                        borderRadius: "4px",
                        marginTop: "4px",
                      }}
                    >
                      <Text>{selectedReport.description}</Text>
                    </div>
                  </div>
                )}
              </Space>
            </Card>

            {/* Admin Review (if exists) */}
            {(selectedReport.reviewedBy || selectedReport.adminNotes) && (
              <Card title="Admin Review" size="small">
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
                      <div
                        style={{
                          background: "#f5f5f5",
                          padding: "8px",
                          borderRadius: "4px",
                          marginTop: "4px",
                        }}
                      >
                        <Text>{selectedReport.adminNotes}</Text>
                      </div>
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

export default memo(ReportsPage);
