import "./CartButton.css";

function CartButton({ itemCount, total, onClick }) {
  return (
    <button className="cart-button" onClick={onClick}>
      <span className="cart-icon">🛒</span>
      {itemCount > 0 && <span className="cart-badge">{itemCount}</span>}
      <div className="cart-button-info">
        <span className="cart-button-label">Cart</span>
        <span className="cart-button-total">${total.toFixed(2)}</span>
      </div>
    </button>
  );
}

export default CartButton;
