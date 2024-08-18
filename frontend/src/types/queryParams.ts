export type WorkQueryParam = {
  sortAsc: boolean;
  showUnitsWithNoRecords: boolean;
  setDefault: boolean;
  startDate?: string;
  endDate?: string;
  timePeriod: string;
  timeMode: string;
  groupBy: string;
  thenBy: string;
};

export type TransportQueryParam = {
  sortAsc: boolean;
  showUnitsWithNoRecords: boolean;
  startDate?: string;
  endDate?: string;
  timePeriod: string;
  timeMode: string;
  groupBy: string;
  thenBy: string;
};

export type AbsenceQueryParam = {
  sortAsc: boolean;
  showUnitsWithNoRecords: boolean;
  startDate?: string;
  endDate?: string;
  timePeriod: string;
  timeMode: string;
  groupBy: string;
  thenBy: string;
  absenceType: string;
};
