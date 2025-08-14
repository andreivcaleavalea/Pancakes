export enum ReportReason {
  Spam = 0,
  Violence = 1,
  SexualContent = 2,
  Harassment = 3,
  HateSpeech = 4,
  Misinformation = 5,
  InappropriateContent = 6,
  CopyrightViolation = 7,
  Other = 8,
}

export enum ReportContentType {
  BlogPost = 0,
  Comment = 1,
}

export enum ReportStatus {
  Pending = 0,
  UnderReview = 1,
  Resolved = 2,
  Dismissed = 3,
}

export interface CreateReportDto {
  contentType: ReportContentType;
  contentId: string;
  reason: ReportReason;
  description?: string;
}

export interface ReportDto {
  id: string;
  reporterId: string;
  reporterName: string;
  reportedUserId: string;
  reportedUserName?: string;
  contentType: ReportContentType;
  contentId: string;
  reason: ReportReason;
  description?: string;
  status: ReportStatus;
  createdAt: string;
  updatedAt: string;
  reviewedBy?: string;
  reviewedAt?: string;
  adminNotes?: string;
  userBanned: boolean;
  contentRemoved: boolean;
  contentTitle?: string;
  contentExcerpt?: string;
}

export interface UpdateReportDto {
  status: ReportStatus;
  adminNotes?: string;
  userBanned?: boolean;
  contentRemoved?: boolean;
}

export const REPORT_REASON_LABELS: Record<ReportReason, string> = {
  [ReportReason.Spam]: "Spam",
  [ReportReason.Violence]: "Violence",
  [ReportReason.SexualContent]: "Sexual Content",
  [ReportReason.Harassment]: "Harassment",
  [ReportReason.HateSpeech]: "Hate Speech",
  [ReportReason.Misinformation]: "Misinformation",
  [ReportReason.InappropriateContent]: "Inappropriate Content",
  [ReportReason.CopyrightViolation]: "Copyright Violation",
  [ReportReason.Other]: "Other",
};

export const REPORT_STATUS_LABELS: Record<ReportStatus, string> = {
  [ReportStatus.Pending]: "Pending",
  [ReportStatus.UnderReview]: "Under Review",
  [ReportStatus.Resolved]: "Resolved",
  [ReportStatus.Dismissed]: "Dismissed",
};
