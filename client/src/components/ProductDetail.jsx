import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getProductById } from "../services/api";
import "./ProductDetail.css";

function ProductDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchProduct = async () => {
      try {
        setLoading(true);
        const data = await getProductById(id);
        setProduct(data);
        setError(null);
      } catch (err) {
        setError("Failed to load product. Please try again later.");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchProduct();
  }, [id]);

  if (loading) {
    return <div className="loading">Loading product...</div>;
  }

  if (error) {
    return <div className="error">{error}</div>;
  }

  if (!product) {
    return <div className="error">Product not found</div>;
  }

  const displayPrice = product.isOnSale ? product.salePrice : product.price;
  const hasDiscount = product.isOnSale && product.salePrice < product.price;

  return (
    <div className="product-detail-container">
      <button onClick={() => navigate("/")} className="back-button">
        ← Back to Products
      </button>

      <div className="product-detail">
        <div className="product-detail-image">
          <img
            src={
              product.imageUrl ||
              "https://via.placeholder.com/600x400?text=No+Image"
            }
            alt={product.name}
          />
          {hasDiscount && <span className="sale-badge-large">SALE</span>}
        </div>

        <div className="product-detail-info">
          <h1>{product.name}</h1>

          <div className="product-detail-pricing">
            {hasDiscount && (
              <span className="original-price-large">
                ${product.price.toFixed(2)}
              </span>
            )}
            <span className="current-price-large">
              ${displayPrice.toFixed(2)}
            </span>
          </div>

          <div className="product-detail-stock">
            {product.currentStock > 0 ? (
              <span className="in-stock-large">
                ✓ In Stock ({product.currentStock} available)
              </span>
            ) : (
              <span className="out-of-stock-large">✗ Out of Stock</span>
            )}
          </div>

          <div className="product-detail-description">
            <h2>Description</h2>
            <p>{product.description}</p>
          </div>

          <div className="product-detail-meta">
            <p>
              <strong>Product ID:</strong> {product.id}
            </p>
            <p>
              <strong>Added:</strong>{" "}
              {new Date(product.createdDate).toLocaleDateString()}
            </p>
            <p>
              <strong>Last Updated:</strong>{" "}
              {new Date(product.lastUpdatedDate).toLocaleDateString()}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}

export default ProductDetail;
