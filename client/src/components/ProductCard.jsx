import { Link } from "react-router-dom";
import "./ProductCard.css";

function ProductCard({ product }) {
  const displayPrice = product.isOnSale ? product.salePrice : product.price;
  const hasDiscount = product.isOnSale && product.salePrice < product.price;

  return (
    <Link to={`/products/${product.id}`} className="product-card">
      <div className="product-image">
        <img
          src={
            product.imageUrl ||
            "https://via.placeholder.com/300x200?text=No+Image"
          }
          alt={product.name}
        />
        {hasDiscount && <span className="sale-badge">SALE</span>}
      </div>
      <div className="product-info">
        <h3 className="product-name">{product.name}</h3>
        <div className="product-pricing">
          {hasDiscount && (
            <span className="original-price">${product.price.toFixed(2)}</span>
          )}
          <span className="current-price">${displayPrice.toFixed(2)}</span>
        </div>
        <p className="product-stock">
          {product.currentStock > 0 ? (
            <span className="in-stock">In Stock ({product.currentStock})</span>
          ) : (
            <span className="out-of-stock">Out of Stock</span>
          )}
        </p>
      </div>
    </Link>
  );
}

export default ProductCard;
