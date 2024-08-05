interface TimeModePeriodObject {
  name: string;
  timespan: string;
  total: number;
}

interface ThenByObject {
  name: string;
  total: number;
}

interface GroupByObject {
  name: string;
  total: number;
  thenByObjects: ThenByObject[];
}

interface TimePeriodObject {
  groupByObjects: GroupByObject[];
}

export interface OverviewQueryResponse {
  type: string;
  queryString: string;
  timeModePeriodObject: TimeModePeriodObject;
  timePeriodObject: TimePeriodObject;
}
