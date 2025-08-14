import { authenticatedBlogRequest } from "@/utils/blogApi";
import type {
  CreateReportDto,
  ReportDto,
  ReportContentType,
} from "@/types/report";

const ENDPOINTS = {
  REPORTS: "/api/reports",
} as const;

export const reportApi = {
  // Create a new report
  create: async (reportData: CreateReportDto): Promise<ReportDto> => {
    return authenticatedBlogRequest<ReportDto>(ENDPOINTS.REPORTS, {
      method: "POST",
      body: JSON.stringify(reportData),
    });
  },

  // Get all reports (admin only)
  getAll: async (
    page: number = 1,
    pageSize: number = 20,
    status?: string
  ): Promise<ReportDto[]> => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    if (status) {
      params.append("status", status);
    }

    return authenticatedBlogRequest<ReportDto[]>(
      `${ENDPOINTS.REPORTS}?${params.toString()}`
    );
  },

  // Get a specific report by ID
  getById: async (id: string): Promise<ReportDto> => {
    return authenticatedBlogRequest<ReportDto>(`${ENDPOINTS.REPORTS}/${id}`);
  },

  // Update a report (admin only)
  update: async (id: string, updateData: any): Promise<ReportDto> => {
    return authenticatedBlogRequest<ReportDto>(`${ENDPOINTS.REPORTS}/${id}`, {
      method: "PUT",
      body: JSON.stringify(updateData),
    });
  },

  // Delete a report (admin only)
  delete: async (id: string): Promise<void> => {
    return authenticatedBlogRequest<void>(`${ENDPOINTS.REPORTS}/${id}`, {
      method: "DELETE",
    });
  },

  // Get my reports
  getMyReports: async (): Promise<ReportDto[]> => {
    return authenticatedBlogRequest<ReportDto[]>(
      `${ENDPOINTS.REPORTS}/my-reports`
    );
  },

  // Check if user can report content
  canReport: async (
    contentType: ReportContentType,
    contentId: string
  ): Promise<boolean> => {
    return authenticatedBlogRequest<boolean>(
      `${ENDPOINTS.REPORTS}/can-report/${contentType}/${contentId}`
    );
  },

  // Get report statistics (admin only)
  getStats: async (): Promise<{
    totalReports: number;
    pendingReports: number;
  }> => {
    return authenticatedBlogRequest<{
      totalReports: number;
      pendingReports: number;
    }>(`${ENDPOINTS.REPORTS}/stats`);
  },
};
