import { useState } from "react";

type SearchableDropdownProps<T> = {
  items: T[];
  selectedItem: T | null;
  setSelectedItem: (item: T | null) => void;
  placeholder: string;
  renderItem: (item: T) => string; // Function to render item label
};

const SearchableDropdown = <T,>({
  items,
  selectedItem,
  setSelectedItem,
  placeholder,
  renderItem,
}: SearchableDropdownProps<T>) => {
  const [searchTerm, setSearchTerm] = useState("");
  const [isOpen, setIsOpen] = useState(false);

  const filteredItems = items.filter((item) =>
    renderItem(item).toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleItemSelect = (item: T) => {
    setSelectedItem(item);
    setIsOpen(false);
  };

  return (
    <div className="relative inline-block w-full">
      <div className="relative">
        <button
          type="button"
          className="w-full bg-white border border-gray-300 rounded-md shadow-sm pl-3 pr-10 py-2 text-left cursor-default focus:outline-none focus:ring-1 focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
          onClick={() => setIsOpen((prev) => !prev)}
        >
          {selectedItem ? renderItem(selectedItem) : placeholder}
          <span className="absolute inset-y-0 right-0 flex items-center pr-2 pointer-events-none">
            <svg
              className="h-5 w-5 text-gray-400"
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 20 20"
              fill="currentColor"
              aria-hidden="true"
            >
              <path
                fillRule="evenodd"
                d="M5.293 7.707a1 1 0 011.414 0L10 11.586l3.293-3.879a1 1 0 111.414 1.414l-4 4.586a1 1 0 01-1.414 0l-4-4.586a1 1 0 010-1.414z"
                clipRule="evenodd"
              />
            </svg>
          </span>
        </button>
      </div>
      {isOpen && (
        <div className="absolute z-10 mt-1 w-full bg-white shadow-lg max-h-60 rounded-md py-1 text-base ring-1 ring-black ring-opacity-5 overflow-auto sm:text-sm">
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Search..."
            className="w-full pl-3 pr-10 py-2 border-b border-gray-200"
          />
          {filteredItems.map((item, index) => (
            <div
              key={index}
              onClick={() => handleItemSelect(item)}
              className="cursor-pointer select-none relative py-2 pl-3 pr-9 hover:bg-gray-200"
            >
              {renderItem(item)}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default SearchableDropdown;
