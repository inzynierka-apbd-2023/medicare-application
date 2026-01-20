import React from "react";
import { Link } from "react-router-dom";
import { Card } from "@shared/components";
import { ArrowLeft, Shield } from "lucide-react";

export const TermsPage: React.FC = () => {
  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="max-w-4xl w-full">
        <Link
          to="/login"
          className="inline-flex items-center text-blue-600 hover:underline mb-6 font-medium"
        >
          <ArrowLeft className="w-4 h-4 mr-2" />
          Back to Login
        </Link>
        <Card variant="medical" padding="lg">
          <div className="flex items-center space-x-3 mb-6 border-b pb-4">
            <div className="p-2 bg-blue-100 rounded-lg">
              <Shield className="w-6 h-6 text-blue-600" />
            </div>
            <h1 className="text-2xl font-bold text-gray-900">
              Terms of Service
            </h1>
          </div>

          <div className="prose prose-blue max-w-none text-gray-600 space-y-6">
            <p>Last updated: January 2, 2026</p>

            <section>
              <h2 className="text-xl font-semibold text-gray-800 mb-3">
                1. Acceptance of Terms
              </h2>
              <p>
                By accessing and using this healthcare platform ("Medicare"),
                you accept and agree to be bound by the terms and provision of
                this agreement.
              </p>
            </section>

            <section>
              <h2 className="text-xl font-semibold text-gray-800 mb-3">
                2. Medical Advice Disclaimer
              </h2>
              <p>
                The content provided on this application is for informational
                purposes only and does not constitute professional medical
                advice, diagnosis, treatment, or recommendations of any kind.
                Always seek the advice of your qualified health providers with
                any questions you may have regarding a medical condition.
              </p>
            </section>

            <section>
              <h2 className="text-xl font-semibold text-gray-800 mb-3">
                3. User Accounts
              </h2>
              <p>
                You are responsible for maintaining the confidentiality of your
                account and password and for restricting access to your
                computer, and you agree to accept responsibility for all
                activities that occur under your account or password.
              </p>
            </section>

            <section>
              <h2 className="text-xl font-semibold text-gray-800 mb-3">
                4. Privacy Policy
              </h2>
              <p>
                Your use of the site is also subject to our Privacy Policy.
                Please review our Privacy Policy, which also governs the Site
                and informs users of our data collection practices.
              </p>
            </section>

            <section>
              <h2 className="text-xl font-semibold text-gray-800 mb-3">
                5. Modifications
              </h2>
              <p>
                We reserve the right to change these terms at any time. Please
                check these terms periodically for changes.
              </p>
            </section>
          </div>
        </Card>
      </div>
    </div>
  );
};
