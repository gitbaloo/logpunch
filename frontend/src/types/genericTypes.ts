type UUID = string;

export interface Client {
  id: UUID;
  name: string;
}

export interface Registration {
  id: UUID;
  employeeId: UUID;
  type: string;
  amount?: number;
  start: string;
  end?: string;
  creatorId: UUID;
  clientId?: UUID;
  creationTime: string;
  status: string;
  firstComment?: Text;
  secondComment?: Text;
  correctionOfId?: UUID;
}

export interface User {
  id: UUID;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}
