import React from "react";
import { Card, Skeleton, Space, Row, Col } from "antd";

// Table skeleton for reports, users, etc.
export const TableSkeleton: React.FC<{ rows?: number }> = ({ rows = 5 }) => (
  <Card>
    <div style={{ marginBottom: 16 }}>
      <Skeleton.Input style={{ width: 200 }} active />
    </div>
    <Space direction="vertical" style={{ width: "100%" }} size="small">
      {Array.from({ length: rows }, (_, index) => (
        <div
          key={index}
          style={{ display: "flex", gap: "16px", padding: "12px 0" }}
        >
          <Skeleton.Avatar size="small" />
          <Skeleton.Input style={{ width: 120 }} active />
          <Skeleton.Input style={{ width: 200 }} active />
          <Skeleton.Input style={{ width: 100 }} active />
          <Skeleton.Input style={{ width: 80 }} active />
          <Skeleton.Button size="small" />
        </div>
      ))}
    </Space>
  </Card>
);

// Stats cards skeleton for dashboard
export const StatsCardsSkeleton: React.FC<{ columns?: number }> = ({
  columns = 4,
}) => (
  <Row gutter={16}>
    {Array.from({ length: columns }, (_, index) => (
      <Col span={24 / columns} key={index}>
        <Card>
          <Skeleton active paragraph={{ rows: 2 }} />
        </Card>
      </Col>
    ))}
  </Row>
);

// Report details skeleton
export const ReportDetailsSkeleton: React.FC = () => (
  <Space direction="vertical" style={{ width: "100%" }} size="large">
    <Card title={<Skeleton.Input style={{ width: 150 }} />} size="small">
      <Row gutter={16}>
        <Col span={12}>
          <Space direction="vertical" style={{ width: "100%" }}>
            <Skeleton.Input style={{ width: "100%" }} />
            <Skeleton.Input style={{ width: "80%" }} />
            <Skeleton.Input style={{ width: "90%" }} />
          </Space>
        </Col>
        <Col span={12}>
          <Space direction="vertical" style={{ width: "100%" }}>
            <Skeleton.Input style={{ width: "100%" }} />
            <Skeleton.Input style={{ width: "70%" }} />
            <Skeleton.Input style={{ width: "85%" }} />
          </Space>
        </Col>
      </Row>
    </Card>

    <Card title={<Skeleton.Input style={{ width: 120 }} />} size="small">
      <Space direction="vertical" style={{ width: "100%" }}>
        <Skeleton.Input style={{ width: "100%" }} />
        <Skeleton.Input style={{ width: "60%" }} />
        <div
          style={{
            background: "#f5f5f5",
            padding: "8px",
            borderRadius: "4px",
            marginTop: "4px",
          }}
        >
          <Skeleton active paragraph={{ rows: 2 }} />
        </div>
      </Space>
    </Card>
  </Space>
);

// Dashboard skeleton
export const DashboardSkeleton: React.FC = () => (
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
        <Skeleton.Input style={{ width: 200, height: 32 }} active />
        <Skeleton.Button active />
      </div>

      {/* Stats Cards */}
      <StatsCardsSkeleton columns={4} />

      {/* Charts/Content */}
      <Row gutter={16}>
        <Col span={12}>
          <Card>
            <Skeleton active paragraph={{ rows: 6 }} />
          </Card>
        </Col>
        <Col span={12}>
          <Card>
            <Skeleton active paragraph={{ rows: 6 }} />
          </Card>
        </Col>
      </Row>
    </Space>
  </div>
);

// Reports page skeleton
export const ReportsPageSkeleton: React.FC = () => (
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
        <Skeleton.Input style={{ width: 180, height: 32 }} active />
        <Skeleton.Button active />
      </div>

      {/* Stats */}
      <StatsCardsSkeleton columns={2} />

      {/* Filters */}
      <Card>
        <div style={{ display: "flex", gap: "16px", alignItems: "center" }}>
          <Skeleton.Input style={{ width: 100 }} />
          <Skeleton.Input style={{ width: 200 }} />
        </div>
      </Card>

      {/* Table */}
      <TableSkeleton rows={8} />
    </Space>
  </div>
);

// User management skeleton
export const UserManagementSkeleton: React.FC = () => (
  <div style={{ padding: "24px" }}>
    <Space direction="vertical" size="large" style={{ width: "100%" }}>
      {/* Header with search */}
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <Skeleton.Input style={{ width: 150, height: 32 }} active />
        <div style={{ display: "flex", gap: "8px" }}>
          <Skeleton.Input style={{ width: 250 }} active />
          <Skeleton.Button active />
        </div>
      </div>

      {/* Stats */}
      <StatsCardsSkeleton columns={3} />

      {/* Filters */}
      <Card>
        <div style={{ display: "flex", gap: "16px", alignItems: "center" }}>
          <Skeleton.Input style={{ width: 120 }} />
          <Skeleton.Input style={{ width: 150 }} />
          <Skeleton.Input style={{ width: 200 }} />
        </div>
      </Card>

      {/* Table */}
      <TableSkeleton rows={10} />
    </Space>
  </div>
);

// Modal skeleton for forms
export const ModalSkeleton: React.FC<{ title?: string }> = ({
  title = "Loading...",
}) => (
  <div>
    <Space direction="vertical" style={{ width: "100%" }} size="large">
      <Card title={title} size="small">
        <Space direction="vertical" style={{ width: "100%" }}>
          <Skeleton.Input style={{ width: "100%" }} active />
          <Skeleton.Input style={{ width: "80%" }} active />
          <Skeleton.Input style={{ width: "90%" }} active />
        </Space>
      </Card>

      <Card size="small">
        <Space direction="vertical" style={{ width: "100%" }}>
          <Skeleton.Input style={{ width: "60%" }} active />
          <Skeleton.Input style={{ width: "70%" }} active />
          <div style={{ marginTop: "16px" }}>
            <Skeleton active paragraph={{ rows: 3 }} />
          </div>
        </Space>
      </Card>

      <div style={{ display: "flex", justifyContent: "flex-end", gap: "8px" }}>
        <Skeleton.Button active />
        <Skeleton.Button active />
        <Skeleton.Button active />
      </div>
    </Space>
  </div>
);

// Generic loading skeleton with customizable layout
export const GenericSkeleton: React.FC<{
  hasHeader?: boolean;
  hasStats?: boolean;
  hasFilters?: boolean;
  hasTable?: boolean;
  statsColumns?: number;
  tableRows?: number;
}> = ({
  hasHeader = true,
  hasStats = false,
  hasFilters = false,
  hasTable = true,
  statsColumns = 4,
  tableRows = 5,
}) => (
  <div style={{ padding: "24px" }}>
    <Space direction="vertical" size="large" style={{ width: "100%" }}>
      {hasHeader && (
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Skeleton.Input style={{ width: 200, height: 32 }} active />
          <Skeleton.Button active />
        </div>
      )}

      {hasStats && <StatsCardsSkeleton columns={statsColumns} />}

      {hasFilters && (
        <Card>
          <div style={{ display: "flex", gap: "16px", alignItems: "center" }}>
            <Skeleton.Input style={{ width: 100 }} />
            <Skeleton.Input style={{ width: 200 }} />
            <Skeleton.Input style={{ width: 150 }} />
          </div>
        </Card>
      )}

      {hasTable && <TableSkeleton rows={tableRows} />}
    </Space>
  </div>
);

export default {
  TableSkeleton,
  StatsCardsSkeleton,
  ReportDetailsSkeleton,
  DashboardSkeleton,
  ReportsPageSkeleton,
  UserManagementSkeleton,
  ModalSkeleton,
  GenericSkeleton,
};

