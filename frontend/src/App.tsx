import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import CustomerSelector from "./routes/CustomerSelector";
import TimeRegistration from "./routes/TimeRegistration";
import ConsultantOverview from "./routes/ConsultantOverview";
import Login from "./routes/Login";
import Query from "./routes/Query";
import Home from "./routes/Home";


const App: React.FC = () => {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="login" element={<Login />} />
        <Route path="customer-selector" element={<CustomerSelector />} />
        <Route path="customers/:customerId" element={<TimeRegistration />} />
        <Route path="overview/query" element={<Query />} />
        <Route path="overview" element={<ConsultantOverview />} />
      </Routes>
    </Router>
  );
};

export default App;
