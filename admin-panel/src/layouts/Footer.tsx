import React from "react";
import { Layout } from "antd";
import "../styles/layouts/Footer.scss";

const { Footer: AntFooter } = Layout;

export const Footer: React.FC = () => {
  const currentYear = new Date().getFullYear();

  return (
    <AntFooter className="footer">
      <p className="footer__text">
        © {currentYear} Pancakes Admin Panel. All rights reserved.
      </p>
      <div className="footer__links">
        <a href="#privacy">Privacy Policy</a>
        <a href="#terms">Terms of Service</a>
        <a href="#support">Support</a>
      </div>
    </AntFooter>
  );
};
