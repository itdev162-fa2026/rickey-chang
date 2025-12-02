import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { createCheckoutSession } from "../../services/api";
import "./Checkout.css";

function Checkout({ cartItems, cartTotal, clearCart }) {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  // Handle empty cart
  if (!cartItems || cartItems.length === 0) {
    return (
      <div className="checkout-container">
        <div className="empty-cart">
          <h2>Your cart is empty</h2>
          <p>Add some products before checking out.</p>
          <button onClick={() => navigate("/")} className="continue-button">
            Continue Shopping
          </button>
        </div>
      </div>
    );
  }

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      // Create checkout session in backend
      const { url } = await createCheckoutSession(email, cartItems);

      if (!url) {
        throw new Error("No checkout URL received from server");
      }

      // Redirect to Stripe Checkout using the URL
      window.location.href = url;

      // Note: If redirect succeeds, user will leave this page
      // Cart will be cleared on the success page after payment
    } catch (err) {
      console.error("Checkout error:", err);
      setError(err.message || "Failed to start checkout. Please try again.");
      setLoading(false);
    }
  };

  return (
    <div className="checkout-container">
      <div className="checkout-content">
        <div className="order-summary">
          <h2>Order Summary</h2>
          <div className="summary-items">
            {cartItems.map((item) => {
              const price = item.product.isOnSale
                ? item.product.salePrice
                : item.product.price;
              const subtotal = price * item.quantity;

              return (
                <div key={item.product.id} className="summary-item">
                  <img
                    src={
                      item.product.imageUrl ||
                      "https://placehold.co/60x60/e0e0e0/666?text=No+Image"
                    }
                    alt={item.product.name}
                    className="summary-item-image"
                  />
                  <div className="summary-item-details">
                    <h4>{item.product.name}</h4>
                    <p>
                      ${price.toFixed(2)} × {item.quantity}
                    </p>
                  </div>
                  <div className="summary-item-total">
                    ${subtotal.toFixed(2)}
                  </div>
                </div>
              );
            })}
          </div>
          <div className="summary-total">
            <span>Total:</span>
            <span className="total-amount">${cartTotal.toFixed(2)}</span>
          </div>
        </div>

        <div className="checkout-form-section">
          <h2>Checkout</h2>
          <form onSubmit={handleSubmit} className="checkout-form">
            <div className="form-group">
              <label htmlFor="email">Email Address</label>
              <input
                type="email"
                id="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                placeholder="your@email.com"
                disabled={loading}
              />
              <p className="form-help">
                We'll send your order confirmation here
              </p>
            </div>

            {error && <div className="error-message">{error}</div>}

            <button
              type="submit"
              className="checkout-button"
              disabled={loading}
            >
              {loading ? "Redirecting to payment..." : "Proceed to Payment"}
            </button>

            <p className="secure-notice">🔒 Secure payment powered by Stripe</p>
          </form>
        </div>
      </div>
    </div>
  );
}

export default Checkout;
