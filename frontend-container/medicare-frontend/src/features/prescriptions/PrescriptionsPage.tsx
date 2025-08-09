import React from "react";

import Header from "../../layout/Header";

import { PrescriptionsFeature } from "./PrescriptionsFeature";

export const PrescriptionsPage: React.FC = () => {
  return (
    <div className="min-h-screen bg-gray-100">
      <Header />
      <div className="pt-20 pb-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <PrescriptionsFeature />
        </div>
      </div>
    </div>
  );
};
