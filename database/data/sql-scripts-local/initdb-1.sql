drop table if exists Consultant;
create table Consultant (
  Id SERIAL PRIMARY KEY,
  First_Name varchar(50) not null,
  Last_Name varchar(50) not null,
  Email varchar(255) not null,
  Password varchar(255) not null,
  Default_Query varchar(255) null
);

drop table if exists Customer;
create table Customer (
  Id SERIAL PRIMARY KEY,
  Name varchar(255) not null
);

drop table if exists Time_Registration;
create table Time_Registration (
  Id SERIAL PRIMARY KEY,
  Hours float8 not null,
  "registration_date" date not null,
  Consultant_CustomerId int4 not null
);

drop table if exists Consultant_Customer;
create table Consultant_Customer (
  Id SERIAL PRIMARY KEY,
  ConsultantId int4 not null,
  CustomerId int4 not null,
  Favorite bool default 'FALSE' not null,
  constraint FKConsultant320576 foreign key (ConsultantId) references Consultant (Id) on delete cascade on update cascade,
  constraint FKCustomer320576 foreign key (CustomerId) references Customer (Id) on delete cascade on update cascade,
  constraint FKTime_Regis573824 foreign key (Id) references Consultant_Customer (Id)
);
