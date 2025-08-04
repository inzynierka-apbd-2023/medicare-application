interface CorporateCTAProps {
  readonly corporateImage: string;
}

export default function CorporateCTA({ corporateImage }: CorporateCTAProps) {
  return (
    <div className="corporate-cta">
      <div className="corporate-banner">
        <img src={corporateImage} alt="Corporate" className="corporate-image" />
        <div className="corporate-content">
          <h2 className="corporate-title">Are you a corporate client?</h2>
          <p className="corporate-subtitle">
            Contact us to learn more about enterprise packages
          </p>
          <button className="corporate-btn">Get in touch</button>
        </div>
      </div>
    </div>
  );
}
