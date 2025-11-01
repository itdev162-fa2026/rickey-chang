import "./CartItem.css";

function CartItem({ item, onUpdateQuantity, onRemove }) {
  const { product, quantity } = item;
  const price = product.isOnSale ? product.salePrice : product.price;
  const subtotal = price * quantity;

  const handleIncrement = () => {
    onUpdateQuantity(product.id, quantity + 1);
  };

  const handleDecrement = () => {
    if (quantity > 1) {
      onUpdateQuantity(product.id, quantity - 1);
    }
  };

  return (
    <div className="cart-item">
      <img
        src={
          product.imageUrl ||
          "https://placehold.co/80x80/e0e0e0/666?text=No+Image"
        }
        alt={product.name}
        className="cart-item-image"
      />
      <div className="cart-item-details">
        <h3 className="cart-item-name">{product.name}</h3>
        <p className="cart-item-price">
          ${price.toFixed(2)}
          {product.isOnSale && (
            <span className="cart-item-sale"> (On Sale!)</span>
          )}
        </p>
      </div>
      <div className="cart-item-controls">
        <div className="quantity-controls">
          <button onClick={handleDecrement} className="quantity-button">
            −
          </button>
          <span className="quantity-display">{quantity}</span>
          <button onClick={handleIncrement} className="quantity-button">
            +
          </button>
        </div>
        <p className="cart-item-subtotal">${subtotal.toFixed(2)}</p>
      </div>
      <button onClick={() => onRemove(product.id)} className="remove-button">
        ✕
      </button>
    </div>
  );
}

export default CartItem;
