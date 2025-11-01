import { useState, useEffect } from "react";
import { searchProducts } from "../services/api";
import ProductCard from "./ProductCard";
import "./ProductList.css";

function ProductList() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [searchInput, setSearchInput] = useState(""); // What user types
  const [activeSearch, setActiveSearch] = useState(""); // What's actually searched

  // Fetch products when activeSearch changes
  useEffect(() => {
    const fetchProducts = async () => {
      try {
        setLoading(true);
        const data = await searchProducts(activeSearch);
        setProducts(data);
        setError(null);
      } catch (err) {
        setError("Failed to load products. Please try again later.");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, [activeSearch]);

  const handleSearch = () => {
    setActiveSearch(searchInput);
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter") {
      handleSearch();
    }
  };

  const handleClearSearch = () => {
    setSearchInput("");
    setActiveSearch("");
  };

  if (loading) {
    return <div className="loading">Loading products...</div>;
  }

  if (error) {
    return <div className="error">{error}</div>;
  }

  return (
    <div className="product-list-container">
      <div className="search-container">
        <input
          type="text"
          placeholder="Search products by name..."
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          onKeyDown={handleKeyDown}
          className="search-input"
        />
        <button onClick={handleSearch} className="search-button">
          Search
        </button>
        {searchInput && (
          <button onClick={handleClearSearch} className="clear-search-button">
            Clear
          </button>
        )}
      </div>

      {products.length === 0 ? (
        <div className="no-results">
          {activeSearch
            ? `No products found matching "${activeSearch}"`
            : "No products available"}
        </div>
      ) : (
        <>
          {activeSearch && (
            <p className="search-results-count">
              Found {products.length} product{products.length !== 1 ? "s" : ""}
            </p>
          )}
          <div className="product-grid">
            {products.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        </>
      )}
    </div>
  );
}

export default ProductList;
