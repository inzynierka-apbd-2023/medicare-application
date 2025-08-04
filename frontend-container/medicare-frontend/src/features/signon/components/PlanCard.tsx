import { Circle } from "lucide-react";

import type { Plan } from "../types";

interface PlanCardProps {
  readonly plan: Plan;
  readonly onSelect: (planName: string) => void;
}

export default function PlanCard({ plan, onSelect }: PlanCardProps) {
  return (
    <div className="plan-card">
      <img src={plan.image} alt={plan.name} className="plan-image" />

      {/* Overlay */}
      <div className="plan-overlay">
        {plan.popular && (
          <button
            className="plan-popular-badge"
            aria-label="Most Popular Plan"
            tabIndex={0}
          >
            Most Popular
          </button>
        )}
        <h2 className="plan-name">{plan.name}</h2>
        <ul className="plan-benefits">
          {plan.benefits.map((benefit) => (
            <li key={benefit} className="plan-benefit">
              <Circle className="icon-bullet" />
              {benefit}
            </li>
          ))}
        </ul>
        <p className="plan-price">{plan.price}</p>
        <button onClick={() => onSelect(plan.name)} className="plan-select-btn">
          Select Plan
        </button>
      </div>
    </div>
  );
}
