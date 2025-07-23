import React from 'react';

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  helperText?: string;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
  variant?: 'default' | 'medical';
}

const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ className, label, error, helperText, leftIcon, rightIcon, variant = 'default', ...props }, ref) => {
    const inputClasses = `
      w-full px-4 py-2 rounded-lg border transition focus:outline-none focus:ring-1
      ${error 
        ? 'border-red-400 focus:border-red-500 focus:ring-red-100' 
        : variant === 'medical'
        ? 'border-blue-500 focus:border-blue-400 focus:ring-blue-100'
        : 'border-gray-200 focus:border-blue-400 focus:ring-blue-100'
      }
      ${leftIcon ? 'pl-10' : ''}
      ${rightIcon ? 'pr-10' : ''}
      ${className || ''}
    `.trim();

    return (
      <div className="w-full">
        {label && (
          <label className="block text-blue-600 font-semibold mb-1" htmlFor={props.id}>
            {label}
          </label>
        )}
        <div className="relative">
          {leftIcon && (
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              {leftIcon}
            </div>
          )}
          <input
            className={inputClasses}
            ref={ref}
            {...props}
          />
          {rightIcon && (
            <div className="absolute inset-y-0 right-0 pr-3 flex items-center">
              {rightIcon}
            </div>
          )}
        </div>
        {error && (
          <p className="mt-1 text-sm text-red-600">{error}</p>
        )}
        {helperText && !error && (
          <p className="mt-1 text-sm text-gray-500">{helperText}</p>
        )}
      </div>
    );
  }
);

Input.displayName = 'Input';

export { Input };
