import { useState, useEffect } from 'react';
import { SieveQueryBuilder } from '../../../../../ts-sieve-plus-query-builder/src/index';
import {type Computer, type ComputerQueryModel, ComputerStoreClient} from "../generated-client.ts";
import {resolveRefs} from "dotnet-json-refs";

const client = new ComputerStoreClient('http://localhost:5284');

export default function Computers() {
  const [computers, setComputers] = useState<Computer[]>([]);
  const [loading, setLoading] = useState(false);

  // Filter state
  const [processors, setProcessors] = useState<string[]>([]);
  const [minPrice, setMinPrice] = useState<number>(500);
  const [maxPrice, setMaxPrice] = useState<number>(4000);
  const [minScreenSize, setMinScreenSize] = useState<number>(13);
  const [maxScreenSize, setMaxScreenSize] = useState<number>(17);
  const [minRam, setMinRam] = useState<number>(8);
  const [inStockOnly, setInStockOnly] = useState(false);
  const [sortBy, setSortBy] = useState<keyof ComputerQueryModel>('price');
  const [sortDesc, setSortDesc] = useState(false);

  const availableProcessors = ['Intel i5', 'Intel i7', 'Intel i9', 'AMD Ryzen 5', 'AMD Ryzen 7', 'AMD Ryzen 9', 'Apple M1', 'Apple M2', 'Apple M3'];

  const fetchComputers = async () => {
    setLoading(true);
    try {
      const builder = SieveQueryBuilder.create<Computer>()
        .page(1)
        .pageSize(50);

      // Processor filter with alternatives (Pricerunner-style)
      if (processors.length > 0) {
        builder.filterWithAlternatives(
          'processor',
          processors,
          (b) => b
            .filterGreaterThanOrEqual('price', minPrice)
            .filterLessThanOrEqual('price', maxPrice)
            .filterGreaterThanOrEqual('screenSize', minScreenSize)
            .filterLessThanOrEqual('screenSize', maxScreenSize)
            .filterGreaterThanOrEqual('ram', minRam)
        );
      } else {
        // No processor selected - just apply other filters
        builder
          .filterGreaterThanOrEqual('price', minPrice)
          .filterLessThanOrEqual('price', maxPrice)
          .filterGreaterThanOrEqual('screenSize', minScreenSize)
          .filterLessThanOrEqual('screenSize', maxScreenSize)
          .filterGreaterThanOrEqual('ram', minRam);
      }

      if (inStockOnly) {
        builder.filterEquals('inStock', true);
      }

      // Sorting
      if (sortDesc) {
        builder.sortByDescending(sortBy as any);
      } else {
        builder.sortBy(sortBy as any);
      }

      const model = builder.buildSieveModel();
      const result = await client.getComputers(model);
      setComputers(resolveRefs(result));
    } catch (error) {
      console.error('Error fetching computers:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchComputers();
  }, [processors, minPrice, maxPrice, minScreenSize, maxScreenSize, minRam, inStockOnly, sortBy, sortDesc]);

  const toggleProcessor = (processor: string) => {
    setProcessors(prev =>
      prev.includes(processor)
        ? prev.filter(p => p !== processor)
        : [...prev, processor]
    );
  };

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-3xl font-bold mb-6">Pricerunner - Computer Comparison</h1>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        {/* Filters */}
        <div className="lg:col-span-1">
          <div className="card bg-base-200 shadow-xl p-4">
            <h2 className="text-xl font-bold mb-4">Filters</h2>

            {/* Processor */}
            <div className="mb-4">
              <h3 className="font-semibold mb-2">Processor</h3>
              {availableProcessors.map(processor => (
                <label key={processor} className="flex items-center gap-2 cursor-pointer mb-1">
                  <input
                    type="checkbox"
                    className="checkbox checkbox-sm"
                    checked={processors.includes(processor)}
                    onChange={() => toggleProcessor(processor)}
                  />
                  <span className="text-sm">{processor}</span>
                </label>
              ))}
            </div>

            {/* Price Range */}
            <div className="mb-4">
              <h3 className="font-semibold mb-2">Price: ${minPrice} - ${maxPrice}</h3>
              <input
                type="range"
                min="500"
                max="4000"
                step="100"
                value={minPrice}
                onChange={(e) => setMinPrice(Number(e.target.value))}
                className="range range-sm mb-2"
              />
              <input
                type="range"
                min="500"
                max="4000"
                step="100"
                value={maxPrice}
                onChange={(e) => setMaxPrice(Number(e.target.value))}
                className="range range-sm"
              />
            </div>

            {/* Screen Size */}
            <div className="mb-4">
              <h3 className="font-semibold mb-2">Screen: {minScreenSize}" - {maxScreenSize}"</h3>
              <input
                type="range"
                min="13"
                max="17"
                step="0.1"
                value={minScreenSize}
                onChange={(e) => setMinScreenSize(Number(e.target.value))}
                className="range range-sm mb-2"
              />
              <input
                type="range"
                min="13"
                max="17"
                step="0.1"
                value={maxScreenSize}
                onChange={(e) => setMaxScreenSize(Number(e.target.value))}
                className="range range-sm"
              />
            </div>

            {/* RAM */}
            <div className="mb-4">
              <h3 className="font-semibold mb-2">Min RAM: {minRam}GB</h3>
              <select
                className="select select-bordered select-sm w-full"
                value={minRam}
                onChange={(e) => setMinRam(Number(e.target.value))}
              >
                <option value="8">8GB</option>
                <option value="16">16GB</option>
                <option value="32">32GB</option>
                <option value="64">64GB</option>
              </select>
            </div>

            {/* In Stock */}
            <div className="mb-4">
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  className="checkbox"
                  checked={inStockOnly}
                  onChange={(e) => setInStockOnly(e.target.checked)}
                />
                <span>In Stock Only</span>
              </label>
            </div>

            {/* Sort */}
            <div className="mb-4">
              <h3 className="font-semibold mb-2">Sort By</h3>
              <select
                className="select select-bordered select-sm w-full mb-2"
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value as keyof ComputerQueryModel)}
              >
                <option value="price">Price</option>
                <option value="rating">Rating</option>
                <option value="screenSize">Screen Size</option>
                <option value="ram">RAM</option>
                <option value="storage">Storage</option>
              </select>
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  className="checkbox checkbox-sm"
                  checked={sortDesc}
                  onChange={(e) => setSortDesc(e.target.checked)}
                />
                <span className="text-sm">Descending</span>
              </label>
            </div>
          </div>
        </div>

        {/* Results */}
        <div className="lg:col-span-3">
          {loading && <div className="text-center">Loading...</div>}

          {!loading && computers.length === 0 && (
            <div className="text-center text-gray-500">No computers found</div>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            {computers && computers.length > 0 && computers.map((computer) => (
              <div key={computer.id} className="card bg-base-100 shadow-xl">
                <div className="card-body">
                  <h2 className="card-title text-lg">{computer.name}</h2>
                  <div className="space-y-1 text-sm">
                    <p><strong>Processor:</strong> {computer.processor}</p>
                    <p><strong>RAM:</strong> {computer.ram}GB</p>
                    <p><strong>Storage:</strong> {computer.storage}GB</p>
                    <p><strong>Screen:</strong> {computer.screenSize}"</p>
                    <p><strong>Graphics:</strong> {computer.graphicsCard}</p>
                    <p className="text-lg font-bold text-primary">${computer.price}</p>
                    <div className="flex items-center gap-2">
                      <div className="badge badge-sm">{computer.rating} ★</div>
                      {computer.inStock ? (
                        <div className="badge badge-success badge-sm">In Stock</div>
                      ) : (
                        <div className="badge badge-error badge-sm">Out of Stock</div>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {!loading && computers.length > 0 && (
            <div className="mt-4 text-center text-sm text-gray-500">
              Showing {computers.length} computer(s)
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
