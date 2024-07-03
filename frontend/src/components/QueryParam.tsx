import { format, endOfWeek, endOfYear, startOfYear } from 'date-fns';

export type QueryParams = {
    sortAsc: boolean;
    showDaysNoRecords: boolean;
    setDefault: boolean;
    startDate: string;
    endDate?: string;
    timePeriod: string;
    timeMode: string;
    groupBy: string;
    thenBy: string;
  };
  
  const defaultParams: QueryParams = {
    sortAsc: false,
    showDaysNoRecords: false,
    setDefault: false,
    startDate: format((new Date()), 'yyyy-MM-dd'),
    endDate: null,
    timePeriod: 'week',
    timeMode: 'current',
    groupBy: 'client',
    thenBy: 'none',
  };

  export default defaultParams;
