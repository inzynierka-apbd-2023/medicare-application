import React from 'react';

export interface IconButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'default' | 'primary' | 'success' | 'warning' | 'danger' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  icon: React.ReactNode;
  tooltip?: string;
}

const IconButton = React.forwardRef<HTMLButtonElement, IconButtonProps>(
  ({ className, variant = 'default', size = 'md', icon, tooltip, ...props }, ref) => {
    const variantClasses = {
      default: 'bg-gray-200 hover:bg-gray-300 text-gray-800',
      primary: 'bg-blue-100 hover:bg-blue-200 text-blue-700',
      success: 'bg-green-100 hover:bg-green-200 text-green-700',
      warning: 'bg-yellow-100 hover:bg-yellow-200 text-yellow-700',
      danger: 'bg-red-100 hover:bg-red-200 text-red-700',
      ghost: 'bg-transparent hover:bg-gray-100 text-gray-700',
    };

    const sizeClasses = {
      sm: 'p-1',
      md: 'p-2',
      lg: 'p-3',
    };

    const buttonClasses = `
      inline-flex items-center justify-center rounded-lg transition
      focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500
      disabled:opacity-50 disabled:cursor-not-allowed
      ${variantClasses[variant]}
      ${sizeClasses[size]}
      ${className || ''}
    `.trim();

    return (
      <button
        className={buttonClasses}
        ref={ref}
        title={tooltip}
        {...props}
      >
        {icon}
      </button>
    );
  }
);

IconButton.displayName = 'IconButton';

export { IconButton };
