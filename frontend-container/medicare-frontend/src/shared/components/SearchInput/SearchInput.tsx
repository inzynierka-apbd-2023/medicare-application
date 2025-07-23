import React from 'react';
import { Search } from 'lucide-react';

export interface SearchInputProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'type'> {
  onSearch?: (value: string) => void;
  loading?: boolean;
  debounceMs?: number;
}

const SearchInput = React.forwardRef<HTMLInputElement, SearchInputProps>(
  ({ className, onSearch, loading, debounceMs = 300, onChange, ...props }, ref) => {
    const [value, setValue] = React.useState(props.value || '');
    const debounceRef = React.useRef<NodeJS.Timeout | null>(null);

    React.useEffect(() => {
      if (onSearch && debounceMs > 0) {
        if (debounceRef.current) {
          clearTimeout(debounceRef.current);
        }
        
        debounceRef.current = setTimeout(() => {
          onSearch(value as string);
        }, debounceMs);

        return () => {
          if (debounceRef.current) {
            clearTimeout(debounceRef.current);
          }
        };
      }
    }, [value, onSearch, debounceMs]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      const newValue = e.target.value;
      setValue(newValue);
      
      if (onChange) {
        onChange(e);
      }

      if (onSearch && debounceMs === 0) {
        onSearch(newValue);
      }
    };

    return (
      <div className="relative flex-1">
        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          {loading ? (
            <svg
              className="animate-spin h-5 w-5 text-gray-400"
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
            >
              <circle
                className="opacity-25"
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                strokeWidth="4"
              />
              <path
                className="opacity-75"
                fill="currentColor"
                d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
              />
            </svg>
          ) : (
            <Search className="h-5 w-5 text-gray-400" />
          )}
        </div>
        <input
          ref={ref}
          type="text"
          className={`
            flex-1 pl-10 pr-3 py-2 bg-white rounded-xl border-none shadow-sm
            focus:outline-none focus:ring-2 focus:ring-blue-500
            ${className || ''}
          `.trim()}
          value={value}
          onChange={handleChange}
          {...props}
        />
      </div>
    );
  }
);

SearchInput.displayName = 'SearchInput';

export { SearchInput };
