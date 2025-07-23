import React, { useState, useRef, useEffect } from 'react';

export interface DropdownItem {
  id: string;
  label: string;
  href?: string;
  onClick?: () => void;
  icon?: React.ReactNode;
  disabled?: boolean;
}

export interface DropdownProps {
  trigger: React.ReactNode;
  items: DropdownItem[];
  align?: 'left' | 'right';
  className?: string;
}

const Dropdown: React.FC<DropdownProps> = ({
  trigger,
  items,
  align = 'right',
  className = '',
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleItemClick = (item: DropdownItem) => {
    if (!item.disabled && item.onClick) {
      item.onClick();
      setIsOpen(false);
    }
  };

  const alignmentClasses = align === 'left' ? 'left-0' : 'right-0';

  return (
    <div className={`relative ${className}`} ref={dropdownRef}>
      <div onClick={() => setIsOpen(!isOpen)}>
        {trigger}
      </div>
      
      {isOpen && (
        <div
          className={`absolute mt-2 w-40 bg-white rounded-lg shadow-lg py-2 z-50 ${alignmentClasses}`}
          role="menu"
          aria-label="Dropdown menu"
        >
          {items.map((item) => (
            <div key={item.id}>
              {item.href ? (
                <a
                  href={item.href}
                  className={`block px-4 py-2 text-gray-700 hover:bg-blue-50 transition ${
                    item.disabled ? 'opacity-50 cursor-not-allowed' : ''
                  }`}
                  role="menuitem"
                  onClick={() => !item.disabled && setIsOpen(false)}
                >
                  <div className="flex items-center gap-2">
                    {item.icon && <span className="flex-shrink-0">{item.icon}</span>}
                    {item.label}
                  </div>
                </a>
              ) : (
                <button
                  className={`w-full text-left block px-4 py-2 text-gray-700 hover:bg-blue-50 transition ${
                    item.disabled ? 'opacity-50 cursor-not-allowed' : ''
                  }`}
                  role="menuitem"
                  onClick={() => handleItemClick(item)}
                  disabled={item.disabled}
                >
                  <div className="flex items-center gap-2">
                    {item.icon && <span className="flex-shrink-0">{item.icon}</span>}
                    {item.label}
                  </div>
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export { Dropdown };
