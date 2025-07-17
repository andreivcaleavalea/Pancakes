import React from 'react';
import { Button } from 'antd';
import { LeftOutlined, RightOutlined } from '@ant-design/icons';
import './Pagination.scss';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

const Pagination: React.FC<PaginationProps> = ({ currentPage, totalPages, onPageChange }) => {
  const handlePrevious = () => {
    if (currentPage > 1) {
      onPageChange(currentPage - 1);
    }
  };

  const handleNext = () => {
    if (currentPage < totalPages) {
      onPageChange(currentPage + 1);
    }
  };

  // Generate page buttons based on totalPages
  const renderPageButtons = () => {
    const buttons = [];
    const maxButtons = 5; // Maximum number of page buttons to show
    
    let startPage = Math.max(1, currentPage - Math.floor(maxButtons / 2));
    const endPage = Math.min(totalPages, startPage + maxButtons - 1);
    
    // Adjust startPage if endPage is at maximum
    if (endPage === totalPages) {
      startPage = Math.max(1, endPage - maxButtons + 1);
    }
    
    // Add first page
    if (startPage > 1) {
      buttons.push(
        <Button
          key={1}
          type="default"
          className={`pagination__page ${currentPage === 1 ? 'pagination__page--active' : ''}`}
          onClick={() => onPageChange(1)}
        >
          1
        </Button>
      );
      
      if (startPage > 2) {
        buttons.push(<span key="ellipsis-1" className="pagination__ellipsis">...</span>);
      }
    }
    
    // Add page buttons
    for (let i = startPage; i <= endPage; i++) {
      buttons.push(
        <Button
          key={i}
          type="default"
          className={`pagination__page ${currentPage === i ? 'pagination__page--active' : ''}`}
          onClick={() => onPageChange(i)}
        >
          {i}
        </Button>
      );
    }
    
    // Add last page
    if (endPage < totalPages) {
      if (endPage < totalPages - 1) {
        buttons.push(<span key="ellipsis-2" className="pagination__ellipsis">...</span>);
      }
      
      buttons.push(
        <Button
          key={totalPages}
          type="default"
          className={`pagination__page ${currentPage === totalPages ? 'pagination__page--active' : ''}`}
          onClick={() => onPageChange(totalPages)}
        >
          {totalPages}
        </Button>
      );
    }
    
    return buttons;
  };

  return (
    <div className="pagination">
      <Button 
        type="default"
        className="pagination__arrow-btn"
        onClick={handlePrevious}
        disabled={currentPage === 1}
      >
        <LeftOutlined className="pagination__icon" />
      </Button>
      
      <div className="pagination__pages">
        {renderPageButtons()}
      </div>
      
      <Button 
        type="default"
        className="pagination__arrow-btn"
        onClick={handleNext}
        disabled={currentPage === totalPages}
      >
        <RightOutlined className="pagination__icon" />
      </Button>
    </div>
  );
};

export default Pagination;
