import { api } from "./api";
import {
  ReportDto,
  UpdateReportDto,
  ReportStats,
  ReportStatus,
} from "@/types/report";
import type { ApiResponse } from "@/types";

export const reportApi = {
  // Get all reports with pagination and filtering
  getAll: async (
    page: number = 1,
    pageSize: number = 20,
    status?: ReportStatus
  ): Promise<ReportDto[]> => {
    const params: Record<string, string> = {
      page: page.toString(),
      pageSize: pageSize.toString(),
    };

    if (status !== undefined) {
      params.status = status.toString();
    }

    const response = await api.get("/reports", { params });
    return response.data.data; // AdminService wraps responses in ApiResponse format
  },

  // Get a specific report by ID
  getById: async (id: string): Promise<ReportDto> => {
    const response = await api.get(`/reports/${id}`);
    return response.data.data;
  },

  // Update a report (resolve, dismiss, etc.)
  update: async (
    id: string,
    updateData: UpdateReportDto
  ): Promise<ReportDto> => {
    const response = await api.put(`/reports/${id}`, updateData);
    return response.data.data;
  },

  // Delete a report
  delete: async (id: string): Promise<void> => {
    await api.delete(`/reports/${id}`);
  },

  // Get report statistics
  getStats: async (): Promise<ReportStats> => {
    const response = await api.get("/reports/stats");
    return response.data.data;
  },
};
