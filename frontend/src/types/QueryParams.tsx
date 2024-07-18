// src/types/QueryParams.ts

export type QueryParam = {
  sortAsc: boolean;
  showUnitsWithNoRecords: boolean;
  setDefault: boolean;
  startDate: string;
  endDate?: string;
  timePeriod: string;
  timeMode: string;
  groupBy: string;
  thenBy: string;
};

const defaultParams: QueryParam = {
  sortAsc: false,
  showUnitsWithNoRecords: false,
  setDefault: false,
  startDate: "",
  endDate: "",
  timePeriod: "week",
  timeMode: "current",
  groupBy: "client",
  thenBy: "none",
};

export default defaultParams;
