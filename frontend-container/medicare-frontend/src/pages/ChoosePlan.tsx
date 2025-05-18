import React, { useRef } from 'react';

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
    <div className="h-screen flex flex-col">
      <h1 className="text-4xl font-bold text-center py-8">Choose Your Plan</h1>

      {/* Plan slider */}
      <div
        ref={scrollRef}
        onMouseDown={onMouseDown}
        onMouseLeave={onMouseLeaveOrUp}
        onMouseUp={onMouseLeaveOrUp}
        onMouseMove={onMouseMove}
        className="flex overflow-x-auto space-x-6 px-6 pb-10 scrollbar-hide cursor-grab"
      >
        {plans.map((plan, idx) => (
          <div
            key={idx}
            className="relative min-w-[80vw] sm:min-w-[60vw] md:min-w-[40vw] h-[65vh] rounded-2xl overflow-hidden transform transition hover:scale-[1.03] brightness-90 hover:brightness-100 shadow-xl"
          >
            <img
              src={plan.image}
              alt={plan.name}
              className="absolute inset-0 w-full h-full object-cover"
            />

            {/* Overlay */}
            <div className="absolute inset-0 bg-black bg-opacity-40 flex flex-col justify-end p-6 text-white">
              {plan.popular && (
                <button
                  className="absolute top-4 left-4 bg-yellow-400 text-black text-xs font-bold px-3 py-1 rounded-full shadow"
                  aria-label="Most Popular Plan"
                  tabIndex={0}
                >
                  Most Popular
                </button>
              )}
              <h2 className="text-2xl font-bold mb-2">{plan.name}</h2>
              <ul className="text-sm mb-3 space-y-1 list-disc list-inside">
                {plan.benefits.map((b, i) => (
                  <li key={i}>{b}</li>
                ))}
              </ul>
              <p className="text-lg font-semibold">{plan.price}</p>
            </div>
          </div>
        ))}
      </div>

      {/* Corporate CTA */}
      <div className="mt-8"> {/* push down from slider */}
        <div className="relative h-[20vh] md:h-[20vh] bg-gray-300"> {/* smaller banner */}
          <img
            src="/assets/pexels-fauxels-3184465.jpg"
            alt="Corporate"
            className="absolute inset-0 w-full h-full object-cover"
          />
          <div className="absolute inset-0 bg-black bg-opacity-50 flex flex-col items-center justify-center text-white px-4 text-center">
            <h2 className="text-3xl sm:text-4xl font-bold mb-2">Are you a corporate client?</h2>
            <p className="text-lg sm:text-xl">Contact us to learn more about enterprise packages</p>
            <button className="mt-4 bg-white text-blue-700 font-bold py-2 px-6 rounded-xl hover:bg-blue-100 transition">
              Get in touch
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
