import React, { useRef } from 'react';
import { Circle } from 'lucide-react';

const plans = [
  {
    name: 'Medicare Standard',
    price: '99 PLN / month',
    benefits: ['Basic consultations', 'Access to GP', 'Online prescriptions'],
    image: '/assets/pexels-pixabay-40568.jpg',
  },
  {
    name: 'Medicare Gold',
    price: '199 PLN / month',
    benefits: ['Everything in Standard', 'Specialist access', 'Shorter wait times'],
    image: '/assets/pexels-thirdman-5327653.jpg',
    popular: true,
  },
  {
    name: 'Medicare Platinum',
    price: '249 PLN / month',
    benefits: ['All in Gold', 'Advanced diagnostics', 'Health concierge'],
    image: '/assets/pexels-shkrabaanthony-5215013.jpg',
  },
  {
    name: 'Medicare Ultimate',
    price: '299 PLN / month',
    benefits: ['All features', 'Private rooms', '24/7 personal care assistant'],
    image: '/assets/pexels-edward-jenner-4031818.jpg',
  },
];

export default function ChoosePlan() {
  const scrollRef = useRef<HTMLDivElement>(null);
  let isDown = false;
  let startX: number;
  let scrollLeft: number;

  const onMouseDown = (e: React.MouseEvent) => {
    isDown = true;
    if (scrollRef.current) {
      scrollRef.current.classList.add('cursor-grabbing');
      startX = e.pageX - scrollRef.current.offsetLeft;
      scrollLeft = scrollRef.current.scrollLeft;
    }
  };

  const onMouseLeaveOrUp = () => {
    isDown = false;
    if (scrollRef.current) {
      scrollRef.current.classList.remove('cursor-grabbing');
    }
  };

  const onMouseMove = (e: React.MouseEvent) => {
    if (!isDown || !scrollRef.current) return;
    e.preventDefault();
    const x = e.pageX - scrollRef.current.offsetLeft;
    const walk = (x - startX) * 1.5;
    scrollRef.current.scrollLeft = scrollLeft - walk;
  };

  return (
    <div className="choose-plan-container">
      <h1 className="choose-plan-title">Choose Your Plan</h1>

      {/* Plan slider */}
      <div
        ref={scrollRef}
        onMouseDown={onMouseDown}
        onMouseLeave={onMouseLeaveOrUp}
        onMouseUp={onMouseLeaveOrUp}
        onMouseMove={onMouseMove}
        className="plan-slider scrollbar-hide"
      >
        {plans.map((plan, idx) => (
          <div key={idx} className="plan-card">
            <img
              src={plan.image}
              alt={plan.name}
              className="plan-image"
            />

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
                {plan.benefits.map((b) => (
                  <li key={b} className="plan-benefit">
                    <Circle className="icon-bullet" />
                    {b}
                  </li>
                ))}
              </ul>
              <p className="plan-price">{plan.price}</p>
              <button
                onClick={() => window.location.href = `/plan-selection?plan=${encodeURIComponent(plan.name)}`}
                className="plan-select-btn"
              >
                Select Plan
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Corporate CTA */}
      <div className="corporate-cta">
        <div className="corporate-banner">
          <img
            src="/assets/pexels-fauxels-3184465.jpg"
            alt="Corporate"
            className="corporate-image"
          />
          <div className="corporate-content">
            <h2 className="corporate-title">Are you a corporate client?</h2>
            <p className="corporate-subtitle">Contact us to learn more about enterprise packages</p>
            <button className="corporate-btn">
              Get in touch
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
