import { DateTime } from "luxon";

type UUID = string;
// Employee requests

// To be used for both work and transport
export interface CreateRegistrationRequest {
  employeeId?: UUID;
  clientId?: UUID;
  start: DateTime;
  end: DateTime;
  firstComment: string;
  secondComment: string;
}

export interface StartRegistrationRequest {
  employeeId?: UUID;
  clientId?: UUID;
  firstComment: string;
}

export interface EndRegistrationRequest {
  employeeId?: UUID;
  registrationId: UUID;
  secondComment: string;
}

export interface EmployeeConfirmationRegistrationRequest {
  registrationId: UUID;
}

export interface EmployeeCorrectionRegistrationRequest {
  clientId?: UUID;
  type: string;
  start: DateTime;
  end: DateTime;
  firstComment: string;
  secondComment: string;
  correctionOfId: UUID;
}

// Admin requests

export interface CreateAbsenceRegistrationRequest {
  employeeId: UUID;
  start: DateTime;
  end: DateTime;
  type: string;
  firstComment: string;
  secondComment: string;
}

export interface UpdateStatusRequest {
  registrationId: UUID;
  status: string;
}

export interface ChangeTypeRequest {
  registrationId: UUID;
  type: string;
}

export interface AdminCorrectionRegistrationRequest {
  employeeId: UUID;
  start: DateTime;
  end: DateTime;
  clientId: UUID;
  firstComment: string;
  secondComment: string;
  correctionOfId: UUID;
}
