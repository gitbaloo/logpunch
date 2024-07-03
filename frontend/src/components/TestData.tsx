import TimeEntry from "./TimeEntry";

const testData: TimeEntry[] = [
  new TimeEntry("Client A", 5, new Date('2024-03-18')),
  new TimeEntry("Client B", 3, new Date('2024-03-19')),
  new TimeEntry("Client A", 4, new Date('2024-03-18')),
  new TimeEntry("Client B", 6, new Date('2024-03-20')),
  new TimeEntry("Client D", 7, new Date('2024-03-21')),
];
export default testData;
