import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Calendar from "../components/Calendar";
import SearchBar from "../components/SearchBar";
import Customer from "../components/Customer";
import LogoutButton from "../components/LogoutButton";
import { useAuthenticatedConsultant } from "../hooks/useAuthenticatedConsultant";
import useFetchCustomers from "../hooks/useFetchCustomers";
import "react-datepicker/dist/react-datepicker.css";
import DatePicker from 'react-datepicker';
import { addDays } from "date-fns";

interface CustomerSelectorProps { }

const CustomerSelector: React.FC<CustomerSelectorProps> = () => {
  const navigate = useNavigate();
  const loggedInConsultant = useAuthenticatedConsultant();
  const [filteredCustomers, setFilteredCustomers] = useState<Customer[]>([]);
  const [startDate, setStartDate] = useState(new Date());

  const { customers = [], isLoading } = useFetchCustomers(
    loggedInConsultant ? loggedInConsultant.id : 0
  );

  useEffect(() => {
    if (loggedInConsultant) {
      setFilteredCustomers(customers);
    }
  }, [customers, loggedInConsultant]);

  const handleSearch = (query: string) => {
    const filtered = customers.filter((customer: Customer) =>
      customer.name.toLowerCase().includes(query.toLowerCase())
    );
    setFilteredCustomers(filtered);
  };

  const handleSelectCustomer = (customer: Customer) => {
    navigate(`/customers/${customer.id}`, {
      state: { customerId: customer.id, customerName: customer.name },
    });
  };


  const goBack = () => {
    navigate("/");
  };

  if (isLoading) {
    return <div>Loading...</div>; // Display loading message while fetching data
  }

  return (
    <>
      <div>
        <div className="grid grid-cols-2 w-full bg-cyan-800 h-20 align-items-center pl-4 text-white">
          <div className="xl:text-lg text-left sm:text-sm col-start-1">
            <h1>{loggedInConsultant?.email}</h1>
          </div>
          <div className="text-end pr-8 ">

            <button className="bg-white hover:bg-gray-200 text-black font-bold py-2 px-3 border border-gray-400 rounded-full shadow" onClick={goBack}>Back</button>
          </div>
        </div>
        <div className="text-center justify-center pt-2">
          <div className="flex justify-center">
              <div>
                <Calendar
                  render={(weekday, date) => (
                    <div>
                      <h1 className="relative w-full flex-none mb-2 text-4xl font-semibold text-teal">{loggedInConsultant?.email}</h1>
                      <h2 className="relative w-full flex-none mb-2 text-xl font-semibold text-teal">Today</h2>
                      <h6>
                        {weekday} {date}
                      </h6>
                    </div>
                  )}
                />

            </div>
          
          </div>
          <div className="pt-3">
            <h4>Select Customer</h4>
          </div>
          <div className="pt-3">
            <SearchBar onSearch={handleSearch} />
          </div>
          <div className="pt-3 flex justify-center align-middle ">
            <ul className="list-decimal">
              {filteredCustomers.map((customer) => (
                <li className="justify-center pl-5" key={customer.id}>
                  <button onClick={() => handleSelectCustomer(customer)}>
                    {customer.name}
                  </button>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </div>

    </>
  );
};

export default CustomerSelector;
