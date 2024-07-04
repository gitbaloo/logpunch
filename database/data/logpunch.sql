create table Consultant (
  Id           int4 default 1 not null, 
  Email        varchar(255) not null, 
  "First Name" varchar(50) not null, 
  "Last Name"  varchar(50) not null, 
  Password     varchar(255) not null, 
  "Default Query" varchar(255) null,
  primary key (Id));
create table Customer (
  Id   int4 not null, 
  Name varchar(255) not null, 
  primary key (Id));
create table Time_Registration (
  Id                    int4 not null, 
  Hours                 float8 not null, 
  "Registration Date"   date not null, 
  Consultant_CustomerId int4 not null, 
  primary key (Id));
create table Consultant_Customer (
  Id            int4 not null, 
  Consultant_Id int4 not null, 
  Customer_Id   int4 not null, 
  Favorite      bool default 'FALSE' not null, 
  primary key (Id));
alter table Consultant_Customer add constraint FKConsultant184203 foreign key (Consultant_Id) references Consultant (Id);
alter table Consultant_Customer add constraint FKConsultant741114 foreign key (Customer_Id) references Customer (Id);
alter table Time_Registration add constraint FKTime_Regis573824 foreign key (Consultant_CustomerId) references Consultant_Customer (Id);
