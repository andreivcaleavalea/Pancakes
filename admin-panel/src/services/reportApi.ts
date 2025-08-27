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
    status?: ReportStatus,
    cacheBustingParams?: Record<string, any>
  ): Promise<ReportDto[]> => {
    const params: Record<string, string> = {
      page: page.toString(),
      pageSize: pageSize.toString(),
    };

    if (status !== undefined) {
      params.status = status.toString();
    }

    // Add cache busting parameters if provided

    const response = await api.get("/reports", {
      params,
      headers: {
        "Cache-Control": "no-cache, no-store, must-revalidate",
        Pragma: "no-cache",
        Expires: "0",
      },
    });
    return response.data.data; // AdminService wraps responses in ApiResponse format
  },

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
  getStats: async (
    cacheBustingParams?: Record<string, any>
  ): Promise<ReportStats> => {
    const config: any = {
      headers: {
        "Cache-Control": "no-cache, no-store, must-revalidate",
        Pragma: "no-cache",
        Expires: "0",
      },
    };

    // Add cache busting parameters if provided
    if (cacheBustingParams) {
      config.params = cacheBustingParams;
    }

    const response = await api.get("/reports/stats", config);
    return response.data.data;
  },
};
