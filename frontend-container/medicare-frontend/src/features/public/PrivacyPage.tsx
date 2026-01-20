import React from "react";
import { Link } from "react-router-dom";
import { Card } from "@shared/components";
import { ArrowLeft, Lock } from "lucide-react";

export const PrivacyPage: React.FC = () => {
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
            <div className="p-2 bg-green-100 rounded-lg">
              <Lock className="w-6 h-6 text-green-600" />
            </div>
            <h1 className="text-2xl font-bold text-gray-900">Privacy Policy</h1>
          </div>

          <div className="prose prose-blue max-w-none text-gray-600 space-y-6">
            <p>Last updated: January 2, 2026</p>

            <section>
              <h2 className="text-xl font-semibold text-gray-800 mb-3">
                1. Information We Collect
              </h2>
              <p>
                We collect information you provide directly to us, such as when
                you create or modify your account, request on-demand services,
                contact customer support, or otherwise communicate with us. This
                information may include: name, email, phone number, postal
                address, profile picture, payment method, and other information
                you choose to provide.
              </p>
            </section>

            <section>
              <h2 className="text-xl font-semibold text-gray-800 mb-3">
                2. How We Use Your Information
              </h2>
              <p>
                We use the information we collect to provide, maintain, and
                improve our services, such as to process appointments,
                facilitate payments, send receipts, provide customer support,
                and send updates and administrative messages.
              </p>
            </section>

            <section>
              <h2 className="text-xl font-semibold text-gray-800 mb-3">
                3. Information Sharing
              </h2>
              <p>
                We may share the information we collect about you as described
                in this Statement or as described at the time of collection or
                sharing, including as follows: with medical professionals to
                schedule appointments, with third party services for payment
                processing, or in response to a request for information if we
                believe disclosure is in accordance with any applicable law.
              </p>
            </section>

            <section>
              <h2 className="text-xl font-semibold text-gray-800 mb-3">
                4. Data Security
              </h2>
              <p>
                We take reasonable measures to help protect information about
                you from loss, theft, misuse and unauthorized access,
                disclosure, alteration and destruction. All medical data is
                encrypted and stored in compliance with relevant healthcare
                regulations.
              </p>
            </section>

            <section>
              <h2 className="text-xl font-semibold text-gray-800 mb-3">
                5. Contact Us
              </h2>
              <p>
                If you have any questions about this Privacy Policy, please
                contact us at privacy@medicare.example.com.
              </p>
            </section>
          </div>
        </Card>
      </div>
    </div>
  );
};
