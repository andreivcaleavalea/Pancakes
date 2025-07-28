import React from 'react';
import { Layout } from 'antd';
import './Footer.scss';

const { Footer: AntFooter } = Layout;

const Footer: React.FC = () => {
  return (
    <AntFooter className="footer">
      <div className="footer__content">
        <div className="footer__copyright">
          © 2025 Pancake. All rights reserved.
        </div>
      </div>
    </AntFooter>
  );
};

export default Footer;
