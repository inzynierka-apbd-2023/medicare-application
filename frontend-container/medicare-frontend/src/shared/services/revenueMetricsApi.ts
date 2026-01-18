import { api } from "./api";

export interface DailyRevenueResponse {
  date: string;
  totalRevenue: number;
  appointmentRevenue: number;
  subscriptionRevenue: number;
  transactionCount: number;
}

export interface MonthlyRevenueResponse {
  year: number;
  month: number;
  totalRevenue: number;
  appointmentRevenue: number;
  subscriptionRevenue: number;
  transactionCount: number;
  growthPercentage?: number;
  dailyBreakdown: DailyRevenueItem[];
}

export interface DailyRevenueItem {
  day: number;
  revenue: number;
  transactionCount: number;
}

export interface YearlyRevenueResponse {
  year: number;
  totalRevenue: number;
  appointmentRevenue: number;
  subscriptionRevenue: number;
  transactionCount: number;
  monthlyBreakdown: MonthlyRevenueItem[];
}

export interface MonthlyRevenueItem {
  month: number;
  revenue: number;
  transactionCount: number;
}

export interface PaymentTypesResponse {
  paymentTypes: PaymentTypeBreakdown[];
  totalRevenue: number;
  totalPaymentCount: number;
  startDate: string;
  endDate: string;
}

export interface PaymentTypeBreakdown {
  paymentType: string;
  revenue: number;
  paymentCount: number;
  percentage: number;
}

export const revenueMetricsApi = {
  getDailyRevenue: async (date?: string): Promise<DailyRevenueResponse> => {
    const params = date ? { date } : {};
    const response = await api.get<DailyRevenueResponse>(
      "/billing/revenue-metrics/daily",
      { params }
    );
    return response;
  },

  getMonthlyRevenue: async (
    year?: number,
    month?: number
  ): Promise<MonthlyRevenueResponse> => {
    const params: Record<string, number> = {};
    if (year) params.year = year;
    if (month) params.month = month;
    const response = await api.get<MonthlyRevenueResponse>(
      "/billing/revenue-metrics/monthly",
      { params }
    );
    return response;
  },

  getYearlyRevenue: async (year?: number): Promise<YearlyRevenueResponse> => {
    const params = year ? { year } : {};
    const response = await api.get<YearlyRevenueResponse>(
      "/billing/revenue-metrics/yearly",
      { params }
    );
    return response;
  },

  getPaymentTypes: async (
    startDate?: string,
    endDate?: string
  ): Promise<PaymentTypesResponse> => {
    const params: Record<string, string> = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    const response = await api.get<PaymentTypesResponse>(
      "/billing/revenue-metrics/payment-types",
      { params }
    );
    return response;
  },
};
//
