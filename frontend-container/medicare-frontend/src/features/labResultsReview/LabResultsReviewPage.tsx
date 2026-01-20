import React from "react";
import Header from "@layout/Header";

import { LabResultsReviewFeature } from "./LabResultsReviewFeature";

export const LabResultsReviewPage: React.FC = () => {
  return (
    <div className="min-h-screen bg-gray-50">
      <Header />
      <main className="pt-20">
        <LabResultsReviewFeature />
      </main>
    </div>
  );
};
