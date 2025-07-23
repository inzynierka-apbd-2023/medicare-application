import React from 'react';

export interface CardProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: 'default' | 'medical' | 'elevated';
  padding?: 'none' | 'sm' | 'md' | 'lg';
  header?: React.ReactNode;
  footer?: React.ReactNode;
}

const Card = React.forwardRef<HTMLDivElement, CardProps>(
  ({ className, variant = 'default', padding = 'md', header, footer, children, ...props }, ref) => {
    const variantClasses = {
      default: 'bg-white rounded-lg shadow-sm border border-gray-200',
      medical: 'bg-white rounded-2xl shadow-md',
      elevated: 'bg-white rounded-2xl shadow-xl',
    };

    const paddingClasses = {
      none: '',
      sm: 'p-4',
      md: 'p-6',
      lg: 'p-8',
    };

    const cardClasses = `${variantClasses[variant]} ${paddingClasses[padding]} ${className || ''}`.trim();

    return (
      <div className={cardClasses} ref={ref} {...props}>
        {header && (
          <div className={`${padding !== 'none' ? 'mb-4' : ''}`}>
            {header}
          </div>
        )}
        
        {children}
        
        {footer && (
          <div className={`${padding !== 'none' ? 'mt-4 pt-4 border-t border-gray-200' : ''}`}>
            {footer}
          </div>
        )}
      </div>
    );
  }
);

Card.displayName = 'Card';

export { Card };
