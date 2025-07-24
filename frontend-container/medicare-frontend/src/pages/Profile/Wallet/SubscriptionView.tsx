import { CheckCircle, XCircle, HeartPulse } from "lucide-react";
import Header from "../../../layout/Header";

export default function SubscriptionView({ subscription, onBuy }) {
  const isActive = subscription?.active;
  const renewal = subscription?.renewalDate
    ? new Date(subscription.renewalDate).toLocaleDateString()
    : null;

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-100 to-blue-50 flex flex-col items-center pb-20 px-2 pt-24 mx-2">
      <Header />
      {/* Icon + Status */}
      <div className="w-full max-w-3xl flex flex-col items-center mb-12 mt-4">
        <div className="flex flex-col items-center">
          <HeartPulse size={56} className="mb-2 text-blue-600 drop-shadow-xl" />
          <div className="flex items-center gap-3 mb-2">
            {isActive ? (
              <>
                <CheckCircle size={36} className="text-green-500" />
                <span className="text-3xl font-extrabold text-green-700 drop-shadow-lg">
                  Your subscription is active
                </span>
              </>
            ) : (
              <>
                <XCircle size={36} className="text-red-400" />
                <span className="text-3xl font-extrabold text-red-500 drop-shadow-lg">
                  No active subscription
                </span>
              </>
            )}
          </div>
          {isActive ? (
            <div className="text-gray-600 text-lg font-medium">
              Plan: <span className="font-semibold">{subscription.type}</span>
              {renewal && (
                <>
                  {" "}
                  | Renewal: <span className="font-semibold">{renewal}</span>
                </>
              )}
            </div>
          ) : (
            <button
              className="mt-5 px-10 py-3 bg-blue-700 text-white rounded-xl text-lg font-bold shadow-md hover:bg-blue-800 transition"
              onClick={onBuy}
            >
              Buy Subscription
            </button>
          )}
        </div>
      </div>

      {/* Welcome & Intro */}
      <div className="w-full max-w-4xl bg-white/70 rounded-3xl shadow-2xl p-12 flex flex-col gap-8 mb-12">
        <h2 className="text-4xl font-extrabold text-blue-800 text-center mb-2 drop-shadow-lg">
          Why choose a medical subscription?
        </h2>
        <p className="text-2xl text-gray-700 text-center leading-relaxed mb-2 font-medium">
          With a subscription, you unlock secure access to all your health
          information, the convenience of digital care, and the support of your
          clinic anytime you need it.
        </p>
        <p className="text-xl text-gray-600 text-center leading-normal max-w-3xl mx-auto">
          Enjoy the peace of mind that comes with managing your healthcare in
          one place. From viewing your history and documents to communicating
          with your doctor or tracking your upcoming visits, your subscription
          is your key to modern, accessible healthcare.
        </p>
      </div>

      {/* Benefits List */}
      <div className="w-full max-w-4xl bg-white rounded-3xl shadow-xl p-10 flex flex-col gap-10">
        <h3 className="text-3xl font-bold text-blue-700 text-center mb-2">
          What does your subscription give you?
        </h3>
        <ul className="text-blue-900 text-xl font-medium flex flex-col gap-7 leading-relaxed">
          <li>
            <span className="font-bold text-blue-600">
              • View your entire medical history:
            </span>
            <br />
            Instantly look up all your past visits, doctors, and medical notes
            at your convenience.
          </li>
          <li>
            <span className="font-bold text-blue-600">
              • Access and download your documents:
            </span>
            <br />
            Your prescriptions, referrals, recommendations, and sick leaves are
            always available digitally.
          </li>
          <li>
            <span className="font-bold text-blue-600">
              • Track treatments and upcoming visits:
            </span>
            <br />
            Easily manage your active prescriptions, care plans, and future
            appointments.
          </li>
          <li>
            <span className="font-bold text-blue-600">
              • Get notifications and reminders:
            </span>
            <br />
            Stay up to date with alerts about new medical documents and
            approaching visits.
          </li>
          <li>
            <span className="font-bold text-blue-600">
              • Review lab results and doctor’s advice:
            </span>
            <br />
            Access all your test results and see your doctor’s recommendations
            in one secure place.
          </li>
          <li>
            <span className="font-bold text-blue-600">
              • Communicate securely with your doctor:
            </span>
            <br />
            Ask questions or clarify your care through private messages to your
            assigned physician or the clinic.
          </li>
          <li>
            <span className="font-bold text-blue-600">
              • Sync with your personal calendar:
            </span>
            <br />
            Add appointments directly to your Microsoft Outlook, Apple Calendar,
            or other personal calendars, so you never miss a visit.
          </li>
          <li>
            <span className="font-bold text-blue-600">
              • Share feedback after visits:
            </span>
            <br />
            Rate your doctor and your experience to help us improve and maintain
            the quality of care.
          </li>
          <li>
            <span className="font-bold text-blue-600">
              • Keep your digital health archive:
            </span>
            <br />
            All your medical documents are securely stored and accessible to you
            at any time.
          </li>
        </ul>
        <div className="mt-8 text-center text-xl text-blue-800 font-semibold">
          By choosing a subscription, you ensure that your health information
          and care are always just a click away—convenient, organized, and
          secure.
        </div>
      </div>
    </div>
  );
}
