import React from "react";

type ParameterBoxProps = {
  title: string;
  children: React.ReactNode;
};

const ParameterBox: React.FC<ParameterBoxProps> = ({ children }) => {
  return (
    <div className="bg-white p-4 rounded-md shadow-md mt-4 max-w-md mx-auto">
      {children}
    </div>
  );
};

export default ParameterBox;
